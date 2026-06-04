#!/usr/bin/env python3
"""
Audio GUID Converter - Usage Examples

Demonstrates workflows for converting audio references in maps
between event:/path format and guids://GUID format.
"""

import json
from pathlib import Path
from audio_guid_converter import AudioGuidConverter


def example_1_convert_single_map():
    """Convert a single exported map JSON to use GUIDs."""
    print("\n" + "="*80)
    print("EXAMPLE 1: Convert Single Map to GUID Format")
    print("="*80)

    converter = AudioGuidConverter()

    # Assumes you have an exported map JSON
    map_file = "01_City.json"
    output_file = "01_City_guid.json"

    success = converter.convert_file(map_file, output_file, reverse=False)

    if success:
        print(f"\n✓ Map converted successfully!")
        print(f"  Input:  {map_file}")
        print(f"  Output: {output_file}")


def example_2_analyze_audio_usage():
    """Analyze all audio references in exported maps."""
    print("\n" + "="*80)
    print("EXAMPLE 2: Analyze Audio Usage in All Maps")
    print("="*80)

    converter = AudioGuidConverter()

    # Analyze all exported maps
    maps_dir = "exports/ASide"
    report_file = "audio_analysis.json"

    analysis = converter.analyze_audio_usage(maps_dir, report_file)

    print(f"\n✓ Analysis complete!")
    print(f"\nTop audio events (by frequency):")
    for event, count in list(analysis['event_frequency'].items())[:10]:
        print(f"  {count:3d}x - {event}")

    if analysis['unmapped_events']:
        print(f"\n⚠ Unmapped events ({len(analysis['unmapped_events'])}):")
        for event in analysis['unmapped_events'][:5]:
            print(f"  - {event}")
        if len(analysis['unmapped_events']) > 5:
            print(f"  ... and {len(analysis['unmapped_events']) - 5} more")


def example_3_batch_convert_chapter():
    """Convert all A-Side maps to GUID format."""
    print("\n" + "="*80)
    print("EXAMPLE 3: Batch Convert Entire Chapter to GUID Format")
    print("="*80)

    converter = AudioGuidConverter()

    # Batch convert
    input_dir = "exports/ASide"
    output_dir = "exports/ASide_guid"

    results = converter.batch_convert(input_dir, output_dir, reverse=False)

    print(f"\n✓ Batch conversion complete!")
    print(f"  Total maps: {results['total']}")
    print(f"  Successful: {results['successful']}")
    print(f"  Failed: {results['failed']}")
    print(f"  Total audio references: {results['total_references']}")
    print(f"  Successfully converted: {results['total_converted']}")


def example_4_complete_workflow():
    """Complete workflow: export → convert → import."""
    print("\n" + "="*80)
    print("EXAMPLE 4: Complete Workflow (Export → Convert → Import)")
    print("="*80)

    from celeste_map_converter import CelesteMapConverter

    map_converter = AudioGuidConverter()
    audio_converter = AudioGuidConverter()

    print("\nStep 1: Export map binary to JSON")
    map_converter = CelesteMapConverter()
    bin_file = "Maps/Maggy/ASide/01_City.bin"
    json_file = "01_City.json"
    map_converter.export_to_json(bin_file, json_file)

    print("\nStep 2: Convert audio references to GUID format")
    guid_json_file = "01_City_guid.json"
    audio_converter.convert_file(json_file, guid_json_file, reverse=False)

    print("\nStep 3: Re-import modified JSON as binary")
    bin_output = "Maps/Maggy/ASide/01_City_guid.bin"
    map_converter.import_from_json(guid_json_file, bin_output)

    print(f"\n✓ Complete workflow finished!")
    print(f"  Original: {bin_file}")
    print(f"  Modified: {bin_output}")


def example_5_find_specific_audio():
    """Find all maps using a specific audio event."""
    print("\n" + "="*80)
    print("EXAMPLE 5: Find Maps Using Specific Audio Event")
    print("="*80)

    import glob

    converter = AudioGuidConverter()
    target_event = "event:/env/amb/01_main"

    print(f"\nSearching for uses of: {target_event}")

    found = []
    for json_file in glob.glob("exports/**/*.json", recursive=True):
        try:
            with open(json_file, 'r') as f:
                data = json.load(f)

            references = converter.find_audio_references(data)
            for path, field, value in references:
                if value == target_event:
                    found.append((json_file, path, field))

        except Exception as e:
            pass

    if found:
        print(f"\n✓ Found {len(found)} references:")
        for json_file, path, field in found[:10]:
            print(f"  - {json_file}")
            print(f"    {field}: {path}")
    else:
        print(f"\n❌ No references found")


def example_6_mass_replace_audio():
    """Replace all instances of an audio event with another."""
    print("\n" + "="*80)
    print("EXAMPLE 6: Mass Replace Audio Event")
    print("="*80)

    import glob

    converter = AudioGuidConverter()

    old_event = "event:/env/amb/01_main"
    new_event = "event:/env/amb/01_main"  # Change this to your new event

    print(f"\nReplacing: {old_event}")
    print(f"       with: {new_event}")

    if old_event not in converter.event_to_guid:
        print(f"⚠ Old event not in GUID mapping: {old_event}")
        return

    if new_event not in converter.event_to_guid:
        print(f"⚠ New event not in GUID mapping: {new_event}")
        return

    replaced = 0
    for json_file in glob.glob("exports/**/*.json", recursive=True):
        try:
            with open(json_file, 'r') as f:
                data = json.load(f)

            references = converter.find_audio_references(data)
            modified = False

            for path, field, value in references:
                if value == old_event:
                    converter._set_nested_value(data, path, new_event)
                    modified = True
                    replaced += 1

            if modified:
                with open(json_file, 'w') as f:
                    json.dump(data, f, indent=2)

        except Exception as e:
            print(f"Error processing {json_file}: {e}")

    print(f"\n✓ Replaced {replaced} audio references")


def example_7_verify_all_mapped():
    """Verify all audio events have GUID mappings."""
    print("\n" + "="*80)
    print("EXAMPLE 7: Verify All Audio Events Are Mapped")
    print("="*80)

    import glob

    converter = AudioGuidConverter()

    unmapped = {}
    for json_file in glob.glob("exports/**/*.json", recursive=True):
        try:
            with open(json_file, 'r') as f:
                data = json.load(f)

            references = converter.find_audio_references(data)
            for path, field, value in references:
                if value.startswith("event://"):
                    if value not in converter.event_to_guid:
                        if value not in unmapped:
                            unmapped[value] = []
                        unmapped[value].append(json_file)

        except Exception as e:
            pass

    if unmapped:
        print(f"\n⚠ Found {len(unmapped)} unmapped events:")
        for event in sorted(unmapped.keys())[:10]:
            count = len(unmapped[event])
            print(f"  {count:3d}x - {event}")
        if len(unmapped) > 10:
            print(f"  ... and {len(unmapped) - 10} more")
    else:
        print(f"\n✓ All audio events are properly mapped!")


def main():
    """Run examples interactively."""
    examples = [
        ("1", "Convert single map to GUID format", example_1_convert_single_map),
        ("2", "Analyze audio usage", example_2_analyze_audio_usage),
        ("3", "Batch convert chapter", example_3_batch_convert_chapter),
        ("4", "Complete workflow", example_4_complete_workflow),
        ("5", "Find specific audio usage", example_5_find_specific_audio),
        ("6", "Mass replace audio event", example_6_mass_replace_audio),
        ("7", "Verify all events mapped", example_7_verify_all_mapped),
    ]

    import sys
    if len(sys.argv) > 1:
        choice = sys.argv[1]
        for num, name, func in examples:
            if num == choice:
                func()
                return
        print(f"Invalid choice: {choice}")

    print("\n" + "="*80)
    print("AUDIO GUID CONVERTER - EXAMPLES")
    print("="*80)
    print("\nAvailable examples:")
    for num, name, _ in examples:
        print(f"  {num}. {name}")
    print("\nUsage:")
    print(f"  python {Path(__file__).name} <number>")
    print(f"\nExample:")
    print(f"  python {Path(__file__).name} 2")


if __name__ == "__main__":
    main()
