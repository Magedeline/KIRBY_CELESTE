# Map Conversion Tools

Tools for converting Celeste map binary format (`.bin`) to JSON and back.

## Tools in This Directory

### celeste_map_converter.py
Pure Python implementation of Celeste map binary format reader/writer. No dependencies.

**Features:**
- Export `.bin` to JSON
- Import JSON back to `.bin`
- Batch operations
- No external dependencies

**Usage:**
```bash
python celeste_map_converter.py export Maps/Maggy/ASide/01_City.bin 01_City.json
python celeste_map_converter.py import 01_City.json Maps/Maggy/ASide/01_City.bin
python celeste_map_converter.py batch-export Maps/Maggy/ASide exports/ASide
python celeste_map_converter.py batch-import exports/ASide Maps/Maggy/ASide
```

### map_converter_examples.py
Example scripts showing common use cases.

**Usage:**
```bash
python map_converter_examples.py 1  # Run example 1
python map_converter_examples.py 3  # Run example 3
```

**Examples:**
1. Export single map
2. Batch export all maps
3. Analyze entities
4. Modify entities
5. Extract all entities
6. Find specific entities

## Loenn Integration

The Lua tool is in `Loenn/tools/MapBinaryExporter.lua`:

```lua
local exporter = require("tools.MapBinaryExporter")
exporter:exportToJson("Maps/Maggy/ASide/01_City.bin", "01_City.json")
exporter:importFromJson("01_City.json", "Maps/Maggy/ASide/01_City.bin")
```

## Quick Start

See [../MAP_CONVERSION_QUICKSTART.md](../MAP_CONVERSION_QUICKSTART.md)

## Format Documentation

See [../MAP_FORMAT_AND_CONVERSION.md](../MAP_FORMAT_AND_CONVERSION.md)

## Requirements

- Python 3.6+ (for Python tools)
- No external dependencies
- Loenn (only for Lua tool)

## Common Commands

```bash
# Single map
python celeste_map_converter.py export input.bin output.json
python celeste_map_converter.py import input.json output.bin

# All maps in directory
python celeste_map_converter.py batch-export Maps/Maggy/ASide exports/ASide
python celeste_map_converter.py batch-import exports/ASide Maps/Maggy/ASide

# Run examples
python map_converter_examples.py 1
python map_converter_examples.py 3
```

---

For detailed information, see the documentation files in the parent directory.
