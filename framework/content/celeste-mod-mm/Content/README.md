# Desolo Zantas Content Directory

This directory contains all visual and audio content for the Desolo Zantas mod.

## Directory Structure

```
Content/
├── Graphics/
│   ├── Atlases/
│   │   └── Gameplay/           # Game sprite atlases
│   │       └── desolo_zantas_mountain.xml
│   └── Sprites/                # Individual sprite files
│       ├── desolo_zantas/      # Mountain logo sprites
│       └── chapters/           # Chapter-specific graphics
├── Dialog/
│   ├── English.txt             # English dialog strings
│   ├── French.txt              # French dialog strings (optional)
│   └── Spanish.txt             # Spanish dialog strings (optional)
└── README.md                   # This file
```

## Content Files

### Graphics/Atlases/Gameplay/desolo_zantas_mountain.xml
Sprite atlas definition for the Desolo Zantas mountain logo and related UI elements.

**Required Sprite Files:**
- `desolo_zantas/mountain_logo.png` (256x256px)
- `desolo_zantas/mountain_logo_small.png` (128x128px)
- `desolo_zantas/mountain_icon.png` (64x64px)
- `desolo_zantas/chapter_select_bg.png` (512x512px)
- `desolo_zantas/mountain_3d_base.png` (512x512px)

### Dialog/English.txt
English language strings for chapter names, mountain names, and UI messages.

## Adding New Content

1. Place sprite PNG files in `Graphics/Sprites/` subdirectories
2. Create corresponding XML atlas definitions in `Graphics/Atlases/Gameplay/`
3. Add dialog strings to `Dialog/English.txt`
4. Build the mod - Everest will automatically load content from this directory

## Notes

- All PNG files should be properly optimized
- Dialog keys should use the `MHELPER_` prefix
- Follow Everest's naming conventions for consistency
- Content is loaded automatically on mod initialization
