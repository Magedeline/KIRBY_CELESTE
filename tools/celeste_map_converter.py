#!/usr/bin/env python3
"""
Celeste Map Binary ↔ JSON Converter

Converts Celeste .bin map files to/from JSON format for inspection and editing.
Works with the Celeste/Everest map format.

Usage:
    # Export binary to JSON
    python celeste_map_converter.py export path/to/map.bin path/to/map.json

    # Import JSON back to binary
    python celeste_map_converter.py import path/to/map.json path/to/map.bin

    # Batch export all maps in a directory
    python celeste_map_converter.py batch-export path/to/maps output/directory

    # Batch import all JSON files to binary
    python celeste_map_converter.py batch-import path/to/json/files output/directory
"""

import json
import struct
import sys
import os
from pathlib import Path
from typing import Any, Dict, List, Tuple, Optional, Union
import logging

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format='[%(levelname)s] %(message)s'
)
logger = logging.getLogger(__name__)


class CelesteMapConverter:
    """Converts Celeste map binaries to/from JSON format."""

    # Celeste binary format constants
    MAGIC = b'CELESTE MAP'
    VERSION = 18  # Celeste 1.4.0

    def __init__(self, verbose: bool = True):
        self.verbose = verbose

    def log(self, message: str, level: str = "info"):
        """Log a message."""
        if self.verbose:
            getattr(logger, level)(message)

    # ========================================================================
    # BINARY READING
    # ========================================================================

    @staticmethod
    def read_string(data: bytes, offset: int) -> Tuple[str, int]:
        """Read a null-terminated string from binary data."""
        end = data.find(b'\x00', offset)
        if end == -1:
            raise ValueError(f"Unterminated string at offset {offset}")
        return data[offset:end].decode('utf-8'), end + 1

    @staticmethod
    def read_int32(data: bytes, offset: int) -> Tuple[int, int]:
        """Read a 32-bit integer."""
        value = struct.unpack('<i', data[offset:offset+4])[0]
        return value, offset + 4

    @staticmethod
    def read_float(data: bytes, offset: int) -> Tuple[float, int]:
        """Read a 32-bit float."""
        value = struct.unpack('<f', data[offset:offset+4])[0]
        return value, offset + 4

    @staticmethod
    def read_bool(data: bytes, offset: int) -> Tuple[bool, int]:
        """Read a boolean value."""
        value = data[offset] != 0
        return value, offset + 1

    @staticmethod
    def read_byte(data: bytes, offset: int) -> Tuple[int, int]:
        """Read a single byte."""
        value = data[offset]
        return value, offset + 1

    def read_entity(self, data: bytes, offset: int) -> Tuple[Dict[str, Any], int]:
        """Read an entity from binary data."""
        entity = {}

        # Read entity type name
        entity['name'], offset = self.read_string(data, offset)

        # Read position
        entity['x'], offset = self.read_int32(data, offset)
        entity['y'], offset = self.read_int32(data, offset)

        # Read size (optional, depends on entity)
        entity['width'], offset = self.read_int32(data, offset)
        entity['height'], offset = self.read_int32(data, offset)

        # Read float properties
        entity['scaleX'], offset = self.read_float(data, offset)
        entity['scaleY'], offset = self.read_float(data, offset)
        entity['rotation'], offset = self.read_float(data, offset)
        entity['depth'], offset = self.read_int32(data, offset)

        # Read boolean properties
        entity['visible'], offset = self.read_bool(data, offset)
        entity['flipX'], offset = self.read_bool(data, offset)
        entity['flipY'], offset = self.read_bool(data, offset)

        # Read custom properties count
        prop_count, offset = self.read_int32(data, offset)
        entity['properties'] = {}

        for _ in range(prop_count):
            # Read property name
            prop_name, offset = self.read_string(data, offset)

            # Read property type
            prop_type, offset = self.read_byte(data, offset)

            # Read property value based on type
            if prop_type == 0:  # String
                value, offset = self.read_string(data, offset)
            elif prop_type == 1:  # Integer
                value, offset = self.read_int32(data, offset)
            elif prop_type == 2:  # Float
                value, offset = self.read_float(data, offset)
            elif prop_type == 3:  # Boolean
                value, offset = self.read_bool(data, offset)
            else:
                logger.warning(f"Unknown property type: {prop_type}")
                value = None

            if value is not None:
                entity['properties'][prop_name] = value

        return entity, offset

    def read_tileset(self, data: bytes, offset: int) -> Tuple[Dict[int, Dict[int, int]], int]:
        """Read tileset/tile data."""
        tiles = {}
        tile_count, offset = self.read_int32(data, offset)

        for _ in range(tile_count):
            x, offset = self.read_int32(data, offset)
            y, offset = self.read_int32(data, offset)
            tile_id, offset = self.read_byte(data, offset)

            if y not in tiles:
                tiles[y] = {}
            tiles[y][x] = tile_id

        return tiles, offset

    def read_room(self, data: bytes, offset: int) -> Tuple[Dict[str, Any], int]:
        """Read a room from binary data."""
        room = {}

        # Read room header
        room['name'], offset = self.read_string(data, offset)
        room['x'], offset = self.read_int32(data, offset)
        room['y'], offset = self.read_int32(data, offset)
        room['width'], offset = self.read_int32(data, offset)
        room['height'], offset = self.read_int32(data, offset)

        # Read room properties
        room['dark'], offset = self.read_bool(data, offset)
        room['music'], offset = self.read_string(data, offset)
        room['ambience'], offset = self.read_string(data, offset)
        room['musicProgress'], offset = self.read_string(data, offset)
        room['windPattern'], offset = self.read_string(data, offset)

        # Read entities
        entity_count, offset = self.read_int32(data, offset)
        room['entities'] = []
        for _ in range(entity_count):
            entity, offset = self.read_entity(data, offset)
            room['entities'].append(entity)

        # Read triggers
        trigger_count, offset = self.read_int32(data, offset)
        room['triggers'] = []
        for _ in range(trigger_count):
            trigger, offset = self.read_entity(data, offset)
            room['triggers'].append(trigger)

        # Read decals
        decal_count, offset = self.read_int32(data, offset)
        room['decals'] = []
        for _ in range(decal_count):
            decal, offset = self.read_entity(data, offset)
            room['decals'].append(decal)

        # Read tilesets
        room['tiles'], offset = self.read_tileset(data, offset)
        room['fgTiles'], offset = self.read_tileset(data, offset)
        room['bgTiles'], offset = self.read_tileset(data, offset)

        return room, offset

    # ========================================================================
    # BINARY WRITING
    # ========================================================================

    @staticmethod
    def write_string(value: str) -> bytes:
        """Write a null-terminated string."""
        return value.encode('utf-8') + b'\x00'

    @staticmethod
    def write_int32(value: int) -> bytes:
        """Write a 32-bit integer."""
        return struct.pack('<i', value)

    @staticmethod
    def write_float(value: float) -> bytes:
        """Write a 32-bit float."""
        return struct.pack('<f', value)

    @staticmethod
    def write_bool(value: bool) -> bytes:
        """Write a boolean value."""
        return bytes([1 if value else 0])

    @staticmethod
    def write_byte(value: int) -> bytes:
        """Write a single byte."""
        return bytes([value & 0xFF])

    def write_entity(self, entity: Dict[str, Any]) -> bytes:
        """Write an entity to binary format."""
        data = b''

        data += self.write_string(entity.get('name', ''))
        data += self.write_int32(entity.get('x', 0))
        data += self.write_int32(entity.get('y', 0))
        data += self.write_int32(entity.get('width', 0))
        data += self.write_int32(entity.get('height', 0))
        data += self.write_float(entity.get('scaleX', 1.0))
        data += self.write_float(entity.get('scaleY', 1.0))
        data += self.write_float(entity.get('rotation', 0.0))
        data += self.write_int32(entity.get('depth', 0))
        data += self.write_bool(entity.get('visible', True))
        data += self.write_bool(entity.get('flipX', False))
        data += self.write_bool(entity.get('flipY', False))

        # Write properties
        properties = entity.get('properties', {})
        data += self.write_int32(len(properties))

        for prop_name, prop_value in properties.items():
            data += self.write_string(prop_name)

            if isinstance(prop_value, str):
                data += self.write_byte(0)  # String type
                data += self.write_string(prop_value)
            elif isinstance(prop_value, bool):
                data += self.write_byte(3)  # Boolean type
                data += self.write_bool(prop_value)
            elif isinstance(prop_value, int):
                data += self.write_byte(1)  # Integer type
                data += self.write_int32(prop_value)
            elif isinstance(prop_value, float):
                data += self.write_byte(2)  # Float type
                data += self.write_float(prop_value)

        return data

    def write_tileset(self, tiles: Dict[int, Dict[int, int]]) -> bytes:
        """Write tileset/tile data."""
        data = b''

        # Count total tiles
        tile_list = []
        for y, row in tiles.items():
            for x, tile_id in row.items():
                tile_list.append((x, y, tile_id))

        data += self.write_int32(len(tile_list))

        for x, y, tile_id in tile_list:
            data += self.write_int32(x)
            data += self.write_int32(y)
            data += self.write_byte(tile_id)

        return data

    def write_room(self, room: Dict[str, Any]) -> bytes:
        """Write a room to binary format."""
        data = b''

        # Write room header
        data += self.write_string(room.get('name', ''))
        data += self.write_int32(room.get('x', 0))
        data += self.write_int32(room.get('y', 0))
        data += self.write_int32(room.get('width', 320))
        data += self.write_int32(room.get('height', 180))

        # Write room properties
        data += self.write_bool(room.get('dark', False))
        data += self.write_string(room.get('music', ''))
        data += self.write_string(room.get('ambience', ''))
        data += self.write_string(room.get('musicProgress', 'persist'))
        data += self.write_string(room.get('windPattern', 'None'))

        # Write entities
        entities = room.get('entities', [])
        data += self.write_int32(len(entities))
        for entity in entities:
            data += self.write_entity(entity)

        # Write triggers
        triggers = room.get('triggers', [])
        data += self.write_int32(len(triggers))
        for trigger in triggers:
            data += self.write_entity(trigger)

        # Write decals
        decals = room.get('decals', [])
        data += self.write_int32(len(decals))
        for decal in decals:
            data += self.write_entity(decal)

        # Write tilesets
        data += self.write_tileset(room.get('tiles', {}))
        data += self.write_tileset(room.get('fgTiles', {}))
        data += self.write_tileset(room.get('bgTiles', {}))

        return data

    # ========================================================================
    # EXPORT/IMPORT
    # ========================================================================

    def export_to_json(self, bin_path: str, json_path: str) -> bool:
        """Export a .bin map file to JSON format."""
        try:
            self.log(f"Exporting {bin_path} to {json_path}")

            # Read binary file
            with open(bin_path, 'rb') as f:
                data = f.read()

            # Verify magic header
            if not data.startswith(self.MAGIC):
                raise ValueError("Invalid Celeste map file (magic header mismatch)")

            offset = len(self.MAGIC)

            # Read version
            version, offset = self.read_int32(data, offset)
            if version != self.VERSION:
                logger.warning(f"Map version {version} may not be fully supported")

            # Create export structure
            export_data = {
                'format_version': '1.0',
                'celeste_version': '1.4.0',
                'map_version': version,
                'rooms': []
            }

            # Read all rooms
            room_count, offset = self.read_int32(data, offset)
            for _ in range(room_count):
                room, offset = self.read_room(data, offset)
                export_data['rooms'].append(room)

            # Write to JSON
            with open(json_path, 'w') as f:
                json.dump(export_data, f, indent=2)

            self.log(f"✓ Exported {len(export_data['rooms'])} rooms to {json_path}")
            return True

        except Exception as e:
            logger.error(f"Export failed: {e}")
            return False

    def import_from_json(self, json_path: str, bin_path: str) -> bool:
        """Import a JSON file and convert to .bin map format."""
        try:
            self.log(f"Importing {json_path} to {bin_path}")

            # Read JSON file
            with open(json_path, 'r') as f:
                import_data = json.load(f)

            # Create binary data
            binary_data = self.MAGIC
            binary_data += self.write_int32(import_data.get('map_version', self.VERSION))

            # Write rooms
            rooms = import_data.get('rooms', [])
            binary_data += self.write_int32(len(rooms))

            for room in rooms:
                binary_data += self.write_room(room)

            # Write to binary file
            with open(bin_path, 'wb') as f:
                f.write(binary_data)

            self.log(f"✓ Imported {len(rooms)} rooms to {bin_path}")
            return True

        except Exception as e:
            logger.error(f"Import failed: {e}")
            return False

    def batch_export(self, maps_dir: str, output_dir: str) -> Dict[str, int]:
        """Export all .bin files in a directory."""
        self.log(f"Batch exporting maps from {maps_dir} to {output_dir}")

        results = {'successful': 0, 'failed': 0}
        maps_dir = Path(maps_dir)
        output_dir = Path(output_dir)
        output_dir.mkdir(parents=True, exist_ok=True)

        for bin_file in maps_dir.rglob('*.bin'):
            json_file = output_dir / bin_file.relative_to(maps_dir).with_suffix('.json')
            json_file.parent.mkdir(parents=True, exist_ok=True)

            if self.export_to_json(str(bin_file), str(json_file)):
                results['successful'] += 1
            else:
                results['failed'] += 1

        self.log(f"Batch export complete: {results['successful']} successful, {results['failed']} failed")
        return results

    def batch_import(self, json_dir: str, output_dir: str) -> Dict[str, int]:
        """Import all .json files and convert to .bin format."""
        self.log(f"Batch importing JSON files from {json_dir} to {output_dir}")

        results = {'successful': 0, 'failed': 0}
        json_dir = Path(json_dir)
        output_dir = Path(output_dir)
        output_dir.mkdir(parents=True, exist_ok=True)

        for json_file in json_dir.rglob('*.json'):
            bin_file = output_dir / json_file.relative_to(json_dir).with_suffix('.bin')
            bin_file.parent.mkdir(parents=True, exist_ok=True)

            if self.import_from_json(str(json_file), str(bin_file)):
                results['successful'] += 1
            else:
                results['failed'] += 1

        self.log(f"Batch import complete: {results['successful']} successful, {results['failed']} failed")
        return results


def main():
    """Command-line interface."""
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)

    command = sys.argv[1]
    converter = CelesteMapConverter()

    if command == 'export' and len(sys.argv) == 4:
        success = converter.export_to_json(sys.argv[2], sys.argv[3])
        sys.exit(0 if success else 1)

    elif command == 'import' and len(sys.argv) == 4:
        success = converter.import_from_json(sys.argv[2], sys.argv[3])
        sys.exit(0 if success else 1)

    elif command == 'batch-export' and len(sys.argv) == 4:
        results = converter.batch_export(sys.argv[2], sys.argv[3])
        sys.exit(0 if results['failed'] == 0 else 1)

    elif command == 'batch-import' and len(sys.argv) == 4:
        results = converter.batch_import(sys.argv[2], sys.argv[3])
        sys.exit(0 if results['failed'] == 0 else 1)

    else:
        print("Invalid command or arguments")
        print(__doc__)
        sys.exit(1)


if __name__ == '__main__':
    main()
