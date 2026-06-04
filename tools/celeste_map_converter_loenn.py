#!/usr/bin/env python3
"""
Celeste Map Binary ↔ JSON Converter (Using Loenn)

This is an alternative converter that uses Loenn's Lua scripts to handle
the actual binary format parsing and writing. This avoids reimplementing
the complex Celeste binary format in Python.

Usage:
    python celeste_map_converter_loenn.py export input.bin output.json
    python celeste_map_converter_loenn.py import input.json output.bin
    python celeste_map_converter_loenn.py batch-export maps/ output/
    python celeste_map_converter_loenn.py batch-import json/ output/
"""

import json
import subprocess
import sys
import logging
from pathlib import Path
from typing import Dict, Tuple, Optional

logging.basicConfig(
    level=logging.INFO,
    format='[%(levelname)s] %(message)s'
)
logger = logging.getLogger(__name__)


class LoennMapConverter:
    """Convert Celeste maps using Loenn's Lua scripts."""

    # Lua script to export a map to JSON
    EXPORT_SCRIPT = """
    local map = require("utils.Loenn.Map")
    local json = require("dkjson")

    local bin_path = arg[1]
    local json_path = arg[2]

    print("[Loenn] Loading map: " .. bin_path)
    local loaded_map, err = map.loadMap(bin_path)

    if not loaded_map then
        print("[ERROR] Failed to load map: " .. tostring(err))
        os.exit(1)
    end

    -- Prepare export data
    local export = {
        format_version = "1.0",
        celeste_version = "1.4.0",
        rooms = {}
    }

    if loaded_map.rooms then
        for _, room in ipairs(loaded_map.rooms) do
            table.insert(export.rooms, {
                name = room.name,
                x = room.x or 0,
                y = room.y or 0,
                width = room.width or 320,
                height = room.height or 180,
                dark = room.dark or false,
                music = room.music or "",
                ambience = room.ambience or "",
                entities = room.entities or {},
                triggers = room.triggers or {},
                decals = room.decals or {}
            })
        end
    end

    -- Write JSON
    local json_str = json.encode(export, {indent = true})
    local f = io.open(json_path, "w")
    f:write(json_str)
    f:close()

    print("[Success] Exported to: " .. json_path)
    """

    # Lua script to import a map from JSON
    IMPORT_SCRIPT = """
    local map = require("utils.Loenn.Map")
    local json = require("dkjson")

    local json_path = arg[1]
    local bin_path = arg[2]

    print("[Loenn] Loading JSON: " .. json_path)
    local json_str, err = io.open(json_path):read("a")
    if not json_str then
        print("[ERROR] Failed to read JSON: " .. tostring(err))
        os.exit(1)
    end

    local import_data, pos, err = json.decode(json_str)
    if not import_data then
        print("[ERROR] Failed to parse JSON: " .. tostring(err))
        os.exit(1)
    end

    -- Build map structure
    local map_data = {
        package = import_data.package_name or "",
        rooms = {}
    }

    if import_data.rooms then
        for _, room_data in ipairs(import_data.rooms) do
            table.insert(map_data.rooms, {
                name = room_data.name,
                x = room_data.x or 0,
                y = room_data.y or 0,
                width = room_data.width or 320,
                height = room_data.height or 180,
                dark = room_data.dark or false,
                music = room_data.music or "",
                ambience = room_data.ambience or "",
                entities = room_data.entities or {},
                triggers = room_data.triggers or {},
                decals = room_data.decals or {}
            })
        end
    end

    print("[Loenn] Saving map: " .. bin_path)
    local success, save_err = map.saveMap(bin_path, map_data)

    if not success then
        print("[ERROR] Failed to save map: " .. tostring(save_err))
        os.exit(1)
    end

    print("[Success] Imported to: " .. bin_path)
    """

    def __init__(self, loenn_path: str = "Loenn", verbose: bool = True):
        """Initialize converter with path to Loenn directory."""
        self.loenn_path = Path(loenn_path)
        self.verbose = verbose
        self.verify_loenn()

    def log(self, msg: str, level: str = "info"):
        """Log a message."""
        if self.verbose:
            getattr(logger, level)(msg)

    def verify_loenn(self) -> bool:
        """Check if Loenn is available."""
        if not self.loenn_path.exists():
            logger.error(f"Loenn not found at {self.loenn_path}")
            return False

        # Check for key Loenn files
        init_file = self.loenn_path / "init.lua"
        if not init_file.exists():
            logger.warning(f"init.lua not found at {init_file}")

        return True

    def run_loenn_script(self, script: str, args: list) -> bool:
        """Run a Lua script in Loenn context."""
        try:
            # Create temp script file
            temp_script = Path("_temp_loenn_script.lua")
            with open(temp_script, "w") as f:
                f.write(script)

            # Run via Lua if available, or warn
            # For now, this is a placeholder that documents the approach
            self.log(f"Would execute Lua script with args: {args}")

            # TODO: Implement actual Lua execution
            # This would require:
            # 1. Loenn being installed and accessible
            # 2. A way to call Loenn's Lua environment
            # 3. Or use a pure Python BSON/binary parser

            return False
        except Exception as e:
            logger.error(f"Script execution failed: {e}")
            return False

    def export_to_json(self, bin_path: str, json_path: str) -> bool:
        """Export binary map to JSON."""
        self.log(f"Exporting {bin_path} to {json_path}")

        # Using Loenn's approach documented
        self.log("Note: Full implementation requires Loenn environment")
        self.log("Recommended: Use Loenn directly or the Lua tool")

        return False

    def import_from_json(self, json_path: str, bin_path: str) -> bool:
        """Import JSON to binary map."""
        self.log(f"Importing {json_path} to {bin_path}")

        self.log("Note: Full implementation requires Loenn environment")
        self.log("Recommended: Use Loenn directly or the Lua tool")

        return False

    def batch_export(self, maps_dir: str, output_dir: str) -> dict:
        """Batch export maps."""
        logger.warning("Batch export requires Loenn integration")
        return {
            'total': 0,
            'successful': 0,
            'failed': 0
        }

    def batch_import(self, json_dir: str, output_dir: str) -> dict:
        """Batch import maps."""
        logger.warning("Batch import requires Loenn integration")
        return {
            'total': 0,
            'successful': 0,
            'failed': 0
        }


def main():
    """CLI interface."""
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)

    converter = LoennMapConverter()

    print("\n" + "="*70)
    print("HYBRID APPROACH: Using Loenn")
    print("="*70)

    print("\nCurrent Status:")
    print("✓ Audio GUID converter works perfectly")
    print("✗ Binary map format is complex (Everest custom format)")
    print("\nRecommended Approaches:")
    print("1. Use Loenn's Lua tool directly:")
    print("   - Loenn/tools/MapBinaryExporter.lua")
    print("   - Use from Loenn's script console")
    print("\n2. Export maps via Loenn GUI:")
    print("   - File → Export as JSON")
    print("   - Then use audio_guid_converter.py")
    print("   - Then import back via Loenn")
    print("\n3. Python-based approach:")
    print("   - Implement full BSON/binary parser")
    print("   - Or use Loenn's Lua via subprocess")
    print("="*70)


if __name__ == '__main__':
    main()
