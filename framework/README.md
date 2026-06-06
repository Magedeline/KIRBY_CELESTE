# Desolo Zantas Mod Framework

Organized framework structure for managing the Desolo Zantas mod and supporting future mods.

## Directory Structure

```
framework/
├── patches/                     # MonoMod IL patches
│   └── monmod-patches/         # Everest-compatible patches
│
├── content/                     # Content packages
│   └── celeste-mod-mm/         # Celeste.Mod.mm content
│       ├── Content/
│       │   ├── Dialog/         # Localized strings
│       │   └── Graphics/       # Atlases and sprites
│       └── metadata.yaml       # Package metadata
│
├── organized/                   # Extracted and organized assets
│   ├── maps/                   # Level maps
│   ├── audio/                  # Audio/music files
│   ├── dialog/                 # Dialog strings
│   ├── graphics/               # Sprites and graphics
│   └── tools/                  # Editor tools (Ahorn/Lönn)
│
└── extracted/                   # Raw extracted zip contents
    ├── Ahorn/                  # Ahorn editor definitions
    ├── Loenn/                  # Lönn editor definitions
    ├── DecalRegistry.xml       # Decal definitions
    └── everest.yaml            # Everest manifest
```

## Mod Safety

The original mod files remain in the root directory:
- `Code/` - Core C# module code
- `Audio/` - Game audio
- `Maps/` - Level files
- `Graphics/` - Game graphics
- `Dialog/` - Dialog files
- `bin/` - Build outputs
- `Patches/` - Patch definitions (backed up in framework)

The `framework/` folder is a **parallel organization** that:
✅ Does NOT override original files
✅ Allows safe experimentation
✅ Supports template extraction
✅ Prevents Everest conflicts

## Usage

All mod systems reference files in their original locations. The framework serves as:
1. Documentation of asset organization
2. Template source for new mods
3. Patch management center
4. Build pipeline configuration

---

## Next Steps (Phase 2)

Move to ModFramework parent directory structure with:
- `templates/` - Mod creation templates
- `shared-libs/` - Reusable systems
- `everest-fork/` - Everest patches
- `mods/` - Individual mod projects
