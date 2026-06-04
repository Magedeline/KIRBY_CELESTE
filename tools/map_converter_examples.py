#!/usr/bin/env python3
"""
Map Converter Usage Examples

Demonstrates common workflows for converting and manipulating Celeste maps.
"""

import json
import sys
from pathlib import Path

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent))

from celeste_map_converter import CelesteMapConverter


def example_1_export_single_map():
    """Export a single map file to JSON for inspection."""
    print("\n" + "="*80)
    print("EXAMPLE 1: Export Single Map")
    print("="*80)

    converter = CelesteMapConverter()

    # Export a map
    bin_path = "Maps/Maggy/ASide/01_City.bin"
    json_path = "01_City.json"

    success = converter.export_to_json(bin_path, json_path)
    if success:
        print(f"\n✓ Map exported successfully!")
        print(f"  View with: cat {json_path} | jq .")

        # Show basic info
        with open(json_path, "r") as f:
            data = json.load(f)
            print(f"\n  Rooms: {len(data['rooms'])}")
            for room in data['rooms'][:3]:
                entities = len(room.get('entities', []))
                triggers = len(room.get('triggers', []))
                print(f"    - {room['name']}: {entities} entities, {triggers} triggers")


def example_2_batch_export_chapter():
    """Export all A-Side maps for batch analysis."""
    print("\n" + "="*80)
    print("EXAMPLE 2: Batch Export All A-Side Maps")
    print("="*80)

    converter = CelesteMapConverter()

    maps_dir = "Maps/Maggy/ASide"
    output_dir = "exports/ASide"

    results = converter.batch_export(maps_dir, output_dir)
    print(f"\n✓ Batch export complete!")
    print(f"  Successful: {results['successful']}")
    print(f"  Failed: {results['failed']}")


def example_3_analyze_entities():
    """Load a map and analyze its entities."""
    print("\n" + "="*80)
    print("EXAMPLE 3: Analyze Entities in a Map")
    print("="*80)

    json_path = "01_City.json"

    try:
        with open(json_path, "r") as f:
            map_data = json.load(f)
    except FileNotFoundError:
        print(f"❌ {json_path} not found. Run Example 1 first.")
        return

    print(f"\nAnalyzing {json_path}...")

    # Collect all entities
    entity_types = {}
    total_entities = 0

    for room in map_data.get("rooms", []):
        for entity in room.get("entities", []):
            entity_type = entity.get("name", "Unknown")
            if entity_type not in entity_types:
                entity_types[entity_type] = 0
            entity_types[entity_type] += 1
            total_entities += 1

    print(f"\nTotal entities: {total_entities}")
    print(f"Entity types ({len(entity_types)}):")

    for entity_type, count in sorted(entity_types.items(), key=lambda x: -x[1]):
        print(f"  - {entity_type}: {count}")

    # Show entity properties example
    if total_entities > 0:
        for room in map_data.get("rooms", []):
            for entity in room.get("entities", []):
                if entity.get("properties"):
                    print(f"\nExample entity with properties:")
                    print(f"  Type: {entity['name']}")
                    print(f"  Position: ({entity['x']}, {entity['y']})")
                    print(f"  Properties:")
                    for key, value in entity["properties"].items():
                        print(f"    - {key}: {value}")
                    break
            else:
                continue
            break


def example_4_modify_entities():
    """Modify entities in a map and re-export to binary."""
    print("\n" + "="*80)
    print("EXAMPLE 4: Modify Entities and Export")
    print("="*80)

    json_path = "01_City.json"
    modified_path = "01_City_modified.json"
    output_bin = "01_City_modified.bin"

    try:
        with open(json_path, "r") as f:
            map_data = json.load(f)
    except FileNotFoundError:
        print(f"❌ {json_path} not found. Run Example 1 first.")
        return

    print(f"\nLoading {json_path}...")

    # Modification example: Add a custom property to all entities
    modified_count = 0
    for room in map_data.get("rooms", []):
        for entity in room.get("entities", []):
            # Example: Track original entity type if it's custom
            if entity["name"].startswith("MAGGYHELPER_"):
                entity["properties"]["modified"] = True
                entity["properties"]["modification_note"] = "Added in example"
                modified_count += 1

    print(f"\nModified {modified_count} entities")

    # Save modified JSON
    with open(modified_path, "w") as f:
        json.dump(map_data, f, indent=2)
    print(f"Saved modified map to: {modified_path}")

    # Convert back to binary
    converter = CelesteMapConverter()
    success = converter.import_from_json(modified_path, output_bin)
    if success:
        print(f"✓ Exported to binary: {output_bin}")


def example_5_extract_all_entities():
    """Extract all entities from all maps for centralized analysis."""
    print("\n" + "="*80)
    print("EXAMPLE 5: Extract All Entities from All Maps")
    print("="*80)

    output_file = "all_entities_summary.json"
    exports_dir = "exports/ASide"

    import glob

    print(f"\nScanning {exports_dir} for JSON files...")

    all_entities = []
    map_count = 0

    for json_file in glob.glob(f"{exports_dir}/*.json"):
        try:
            with open(json_file, "r") as f:
                map_data = json.load(f)
                map_count += 1

                for room in map_data.get("rooms", []):
                    for entity in room.get("entities", []):
                        entity["map_file"] = Path(json_file).stem
                        entity["room_name"] = room["name"]
                        all_entities.append(entity)
        except Exception as e:
            print(f"  ⚠ Error reading {json_file}: {e}")

    print(f"Extracted {len(all_entities)} entities from {map_count} maps")

    # Save summary
    summary = {
        "total_entities": len(all_entities),
        "total_maps": map_count,
        "entities_by_type": {},
        "sample_entities": {}
    }

    for entity in all_entities:
        entity_type = entity.get("name", "Unknown")
        if entity_type not in summary["entities_by_type"]:
            summary["entities_by_type"][entity_type] = 0
            summary["sample_entities"][entity_type] = entity
        summary["entities_by_type"][entity_type] += 1

    with open(output_file, "w") as f:
        json.dump(summary, f, indent=2)

    print(f"✓ Summary saved to: {output_file}")
    print(f"\nTop entity types:")
    for entity_type, count in sorted(
        summary["entities_by_type"].items(), key=lambda x: -x[1]
    )[:10]:
        print(f"  - {entity_type}: {count}")


def example_6_find_specific_entities():
    """Search for entities with specific properties."""
    print("\n" + "="*80)
    print("EXAMPLE 6: Find Entities with Specific Properties")
    print("="*80)

    json_path = "01_City.json"

    try:
        with open(json_path, "r") as f:
            map_data = json.load(f)
    except FileNotFoundError:
        print(f"❌ {json_path} not found. Run Example 1 first.")
        return

    print(f"\nSearching for entities in {json_path}...\n")

    # Example 1: Find all MAGGYHELPER entities
    maggy_entities = []
    for room in map_data.get("rooms", []):
        for entity in room.get("entities", []):
            if entity["name"].startswith("MAGGYHELPER_"):
                maggy_entities.append({
                    "room": room["name"],
                    "entity": entity
                })

    print(f"Found {len(maggy_entities)} MAGGYHELPER entities")
    if maggy_entities:
        for entry in maggy_entities[:3]:
            print(f"  - {entry['entity']['name']} in room {entry['room']}")

    # Example 2: Find entities with specific property
    print(f"\nSearching for entities with 'dialogKey' property...")
    dialog_entities = []
    for room in map_data.get("rooms", []):
        for entity in room.get("entities", []):
            if "dialogKey" in entity.get("properties", {}):
                dialog_entities.append({
                    "room": room["name"],
                    "entity": entity
                })

    print(f"Found {len(dialog_entities)} entities with dialogKey")
    if dialog_entities:
        for entry in dialog_entities[:3]:
            key = entry["entity"]["properties"]["dialogKey"]
            print(f"  - {entry['entity']['name']} (key: {key}) in room {entry['room']}")


def main():
    """Run all examples."""
    examples = [
        ("1", "Export single map", example_1_export_single_map),
        ("2", "Batch export chapter", example_2_batch_export_chapter),
        ("3", "Analyze entities", example_3_analyze_entities),
        ("4", "Modify entities", example_4_modify_entities),
        ("5", "Extract all entities", example_5_extract_all_entities),
        ("6", "Find specific entities", example_6_find_specific_entities),
    ]

    if len(sys.argv) > 1:
        choice = sys.argv[1]
        for num, name, func in examples:
            if num == choice:
                func()
                return
        print(f"Invalid choice: {choice}")

    print("\n" + "="*80)
    print("MAP CONVERTER EXAMPLES")
    print("="*80)
    print("\nAvailable examples:")
    for num, name, _ in examples:
        print(f"  {num}. {name}")
    print("\nUsage:")
    print(f"  python {Path(__file__).name} <number>")
    print(f"\nExample:")
    print(f"  python {Path(__file__).name} 1")


if __name__ == "__main__":
    main()
