#!/usr/bin/env python3
"""
Celeste Audio Event to GUID Converter

Converts audio event references in map JSON files from 'event:/path' format
to 'guids://GUID' format using the Audio/guids.txt mapping file.

Usage:
    # Convert a single map JSON file
    python audio_guid_converter.py convert maps/01_City.json

    # Convert all JSON files in a directory
    python audio_guid_converter.py batch-convert maps/ output/

    # Reverse convert (GUID back to event paths)
    python audio_guid_converter.py reverse maps/01_City.json

    # Analyze audio usage
    python audio_guid_converter.py analyze maps/ --output report.json
"""

import json
import re
import sys
from pathlib import Path
from typing import Dict, List, Tuple, Optional
import logging

logging.basicConfig(
    level=logging.INFO,
    format='[%(levelname)s] %(message)s'
)
logger = logging.getLogger(__name__)


class AudioGuidConverter:
    """Converts between audio event paths and GUIDs in map JSON files."""

    def __init__(self, guids_file: str = "Audio/guids.txt", verbose: bool = True):
        self.verbose = verbose
        self.guids_file = guids_file

        # Mappings: event path ↔ GUID
        self.event_to_guid: Dict[str, str] = {}
        self.guid_to_event: Dict[str, str] = {}

        self.load_guids()

    def log(self, message: str, level: str = "info"):
        """Log a message."""
        if self.verbose:
            getattr(logger, level)(message)

    def load_guids(self) -> bool:
        """Load GUID mappings from guids.txt file."""
        try:
            self.log(f"Loading GUID mappings from {self.guids_file}")

            with open(self.guids_file, 'r') as f:
                for line_num, line in enumerate(f, 1):
                    line = line.strip()
                    if not line or line.startswith('#'):
                        continue

                    # Parse: {GUID} event:/path
                    match = re.match(r'\{([a-f0-9\-]+)\}\s+(.+)', line)
                    if match:
                        guid = match.group(1)
                        event_path = match.group(2).strip()

                        self.event_to_guid[event_path] = guid
                        self.guid_to_event[guid] = event_path
                    else:
                        logger.warning(f"Line {line_num}: Could not parse: {line}")

            self.log(f"✓ Loaded {len(self.event_to_guid)} GUID mappings")
            return True

        except FileNotFoundError:
            logger.error(f"❌ GUID file not found: {self.guids_file}")
            return False
        except Exception as e:
            logger.error(f"❌ Error loading GUIDs: {e}")
            return False

    def find_audio_references(self, data: dict, path: str = "") -> List[Tuple[str, str, str]]:
        """
        Find all audio references in JSON data.
        Returns list of (path, field_name, value) tuples.
        """
        references = []

        def search_recursive(obj, current_path=""):
            if isinstance(obj, dict):
                for key, value in obj.items():
                    new_path = f"{current_path}.{key}" if current_path else key

                    # Check if this is an audio reference field
                    if isinstance(value, str) and (
                        value.startswith("event:/") or
                        value.startswith("guids://")
                    ):
                        references.append((new_path, key, value))
                    elif isinstance(value, (dict, list)):
                        search_recursive(value, new_path)
            elif isinstance(obj, list):
                for idx, item in enumerate(obj):
                    new_path = f"{current_path}[{idx}]"
                    search_recursive(item, new_path)

        search_recursive(data, path)
        return references

    def event_to_guid_format(self, event_path: str) -> Optional[str]:
        """Convert event:/path to guids://GUID format."""
        if event_path.startswith("guids://"):
            # Already in GUID format
            return event_path

        if event_path.startswith("event:/"):
            # Look up in mapping
            if event_path in self.event_to_guid:
                guid = self.event_to_guid[event_path]
                return f"guids://{guid}"
            else:
                logger.warning(f"GUID mapping not found for: {event_path}")
                return None

        return None

    def guid_to_event_format(self, guid_path: str) -> Optional[str]:
        """Convert guids://GUID back to event:/path format."""
        if guid_path.startswith("event:/"):
            # Already in event format
            return guid_path

        if guid_path.startswith("guids://"):
            # Extract GUID
            guid = guid_path.replace("guids://", "")
            if guid in self.guid_to_event:
                return self.guid_to_event[guid]
            else:
                logger.warning(f"Event mapping not found for GUID: {guid}")
                return None

        return None

    def convert_map_to_guid(self, map_data: dict) -> Tuple[dict, dict]:
        """
        Convert all audio references in a map from event to GUID format.
        Returns (modified_map, conversion_report).
        """
        report = {
            'total_references': 0,
            'converted': 0,
            'failed': 0,
            'already_guid': 0,
            'conversions': []
        }

        references = self.find_audio_references(map_data)

        for path, field_name, value in references:
            report['total_references'] += 1

            if value.startswith("guids://"):
                report['already_guid'] += 1
                continue

            guid_value = self.event_to_guid_format(value)
            if guid_value:
                # Update the value in the map using the path
                self._set_nested_value(map_data, path, guid_value)
                report['converted'] += 1
                report['conversions'].append({
                    'path': path,
                    'from': value,
                    'to': guid_value
                })
            else:
                report['failed'] += 1

        return map_data, report

    def convert_map_to_event(self, map_data: dict) -> Tuple[dict, dict]:
        """
        Convert all audio references in a map from GUID back to event format.
        Returns (modified_map, conversion_report).
        """
        report = {
            'total_references': 0,
            'converted': 0,
            'failed': 0,
            'already_event': 0,
            'conversions': []
        }

        references = self.find_audio_references(map_data)

        for path, field_name, value in references:
            report['total_references'] += 1

            if value.startswith("event:/"):
                report['already_event'] += 1
                continue

            event_value = self.guid_to_event_format(value)
            if event_value:
                # Update the value in the map using the path
                self._set_nested_value(map_data, path, event_value)
                report['converted'] += 1
                report['conversions'].append({
                    'path': path,
                    'from': value,
                    'to': event_value
                })
            else:
                report['failed'] += 1

        return map_data, report

    @staticmethod
    def _set_nested_value(data: dict, path: str, value: str):
        """Set a value in nested dict/list using dot notation path."""
        keys = re.split(r'[\.\[\]]', path)
        keys = [k for k in keys if k]  # Remove empty strings

        current = data
        for key in keys[:-1]:
            if isinstance(current, dict):
                if key not in current:
                    current[key] = {}
                current = current[key]
            elif isinstance(current, list):
                idx = int(key)
                current = current[idx]

        final_key = keys[-1]
        if isinstance(current, dict):
            current[final_key] = value
        elif isinstance(current, list):
            current[int(final_key)] = value

    def convert_file(self, input_file: str, output_file: str, reverse: bool = False) -> bool:
        """Convert a map JSON file."""
        try:
            self.log(f"Converting {input_file}...")

            # Read JSON
            with open(input_file, 'r') as f:
                map_data = json.load(f)

            # Convert
            if reverse:
                map_data, report = self.convert_map_to_event(map_data)
                direction = "GUID → Event"
            else:
                map_data, report = self.convert_map_to_guid(map_data)
                direction = "Event → GUID"

            # Write back
            with open(output_file, 'w') as f:
                json.dump(map_data, f, indent=2)

            # Report
            self.log(f"✓ {direction}")
            self.log(f"  Total references: {report['total_references']}")
            self.log(f"  Converted: {report['converted']}")
            self.log(f"  Failed: {report['failed']}")

            return True

        except Exception as e:
            logger.error(f"❌ Error converting {input_file}: {e}")
            return False

    def batch_convert(self, input_dir: str, output_dir: str, reverse: bool = False) -> dict:
        """Batch convert all JSON files in a directory."""
        direction = "GUID → Event" if reverse else "Event → GUID"
        self.log(f"Batch converting {input_dir} ({direction})")

        results = {
            'total': 0,
            'successful': 0,
            'failed': 0,
            'total_references': 0,
            'total_converted': 0,
            'details': []
        }

        input_path = Path(input_dir)
        output_path = Path(output_dir)
        output_path.mkdir(parents=True, exist_ok=True)

        for json_file in input_path.rglob('*.json'):
            results['total'] += 1

            # Determine output path
            rel_path = json_file.relative_to(input_path)
            out_file = output_path / rel_path
            out_file.parent.mkdir(parents=True, exist_ok=True)

            try:
                # Read and convert
                with open(json_file, 'r') as f:
                    map_data = json.load(f)

                if reverse:
                    map_data, report = self.convert_map_to_event(map_data)
                else:
                    map_data, report = self.convert_map_to_guid(map_data)

                # Write output
                with open(out_file, 'w') as f:
                    json.dump(map_data, f, indent=2)

                results['successful'] += 1
                results['total_references'] += report['total_references']
                results['total_converted'] += report['converted']

                results['details'].append({
                    'file': str(json_file),
                    'status': 'success',
                    'references': report['total_references'],
                    'converted': report['converted'],
                    'failed': report['failed']
                })

            except Exception as e:
                results['failed'] += 1
                results['details'].append({
                    'file': str(json_file),
                    'status': 'failed',
                    'error': str(e)
                })

        self.log(f"Batch conversion complete: {results['successful']} successful, {results['failed']} failed")
        self.log(f"Total audio references: {results['total_references']}")
        self.log(f"Total converted: {results['total_converted']}")

        return results

    def analyze_audio_usage(self, input_dir: str, output_file: str = None) -> dict:
        """Analyze audio usage across all maps."""
        self.log(f"Analyzing audio usage in {input_dir}")

        analysis = {
            'total_maps': 0,
            'total_references': 0,
            'event_format': 0,
            'guid_format': 0,
            'unmapped_events': [],
            'event_frequency': {},
            'guid_frequency': {}
        }

        input_path = Path(input_dir)

        for json_file in input_path.rglob('*.json'):
            try:
                analysis['total_maps'] += 1

                with open(json_file, 'r') as f:
                    map_data = json.load(f)

                references = self.find_audio_references(map_data)
                analysis['total_references'] += len(references)

                for path, field_name, value in references:
                    if value.startswith("event:/"):
                        analysis['event_format'] += 1

                        # Track frequency
                        if value not in analysis['event_frequency']:
                            analysis['event_frequency'][value] = 0
                        analysis['event_frequency'][value] += 1

                        # Check if mapped
                        if value not in self.event_to_guid:
                            if value not in analysis['unmapped_events']:
                                analysis['unmapped_events'].append(value)

                    elif value.startswith("guids://"):
                        analysis['guid_format'] += 1

                        # Track frequency
                        guid = value.replace("guids://", "")
                        if value not in analysis['guid_frequency']:
                            analysis['guid_frequency'][value] = 0
                        analysis['guid_frequency'][value] += 1

            except Exception as e:
                logger.warning(f"Error analyzing {json_file}: {e}")

        # Sort by frequency
        analysis['event_frequency'] = dict(
            sorted(analysis['event_frequency'].items(), key=lambda x: -x[1])[:50]
        )
        analysis['guid_frequency'] = dict(
            sorted(analysis['guid_frequency'].items(), key=lambda x: -x[1])[:50]
        )

        self.log(f"✓ Analysis complete")
        self.log(f"  Total maps: {analysis['total_maps']}")
        self.log(f"  Total references: {analysis['total_references']}")
        self.log(f"  Event format: {analysis['event_format']}")
        self.log(f"  GUID format: {analysis['guid_format']}")
        self.log(f"  Unmapped events: {len(analysis['unmapped_events'])}")

        # Save analysis if requested
        if output_file:
            with open(output_file, 'w') as f:
                json.dump(analysis, f, indent=2)
            self.log(f"  Saved to: {output_file}")

        return analysis


def main():
    """Command-line interface."""
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)

    command = sys.argv[1]
    converter = AudioGuidConverter()

    if command == 'convert' and len(sys.argv) == 3:
        input_file = sys.argv[2]
        output_file = input_file.replace('.json', '_guid.json')
        success = converter.convert_file(input_file, output_file, reverse=False)
        sys.exit(0 if success else 1)

    elif command == 'reverse' and len(sys.argv) == 3:
        input_file = sys.argv[2]
        output_file = input_file.replace('.json', '_event.json')
        success = converter.convert_file(input_file, output_file, reverse=True)
        sys.exit(0 if success else 1)

    elif command == 'batch-convert' and len(sys.argv) == 4:
        results = converter.batch_convert(sys.argv[2], sys.argv[3], reverse=False)
        sys.exit(0 if results['failed'] == 0 else 1)

    elif command == 'batch-reverse' and len(sys.argv) == 4:
        results = converter.batch_convert(sys.argv[2], sys.argv[3], reverse=True)
        sys.exit(0 if results['failed'] == 0 else 1)

    elif command == 'analyze' and len(sys.argv) >= 3:
        input_dir = sys.argv[2]
        output_file = None
        if len(sys.argv) >= 5 and sys.argv[3] == '--output':
            output_file = sys.argv[4]
        converter.analyze_audio_usage(input_dir, output_file)
        sys.exit(0)

    else:
        print("Invalid command or arguments")
        print(__doc__)
        sys.exit(1)


if __name__ == '__main__':
    main()
