# Cairn Dev Unlocker - Technical Documentation

## Overview

This mod enables the hidden developer debug menu in Cairn by patching IL2CPP runtime memory.

## How It Works

### Injection Method

Uses [Unity Doorstop](https://github.com/NeighTools/UnityDoorstop) (winhttp.dll proxy) to load a .NET assembly before the game starts. The mod runs on a background thread and waits for the game's IL2CPP runtime to initialize.

### Memory Patching

The mod locates and patches these fields:

| Class           | Field           | Offset  | Patch       |
| --------------- | --------------- | ------- | ----------- |
| `DebugMenuUI`   | `IsEnabled`     | `0x150` | `true` (1)  |
| `DebugMenuData` | `startDisabled` | `0x19`  | `false` (0) |

### Additional Features

- **Time Freeze**: Calls `Time.set_timeScale(0)` via direct RVA call at `0x3ADABF0`
- **Cursor Lock**: Uses Windows API (`ShowCursor`, `SetCursor`)

## RVA Addresses (Cairn 1.0)

These addresses are specific to the current game version and will need updating if the game updates:

```
FindObjectOfType:     0x3AD11E0
Time.set_timeScale:   0x3ADABF0
```

## Building

Requirements:

- .NET 6.0 SDK
- Windows

```bash
cd src
dotnet build -c Release -o ../release
```

## Files Structure

```
CairnDevUnlocker-GitHub/
├── src/
│   ├── CairnDevUnlocker.cs    # Main source code
│   └── CairnDevUnlocker.csproj
├── release/
│   ├── CairnDevUnlocker.dll   # Compiled mod
│   ├── doorstop_config.ini    # Doorstop configuration
│   └── winhttp.dll            # Doorstop loader
├── docs/
│   └── TECHNICAL.md           # This file
├── README.md
└── LICENSE
```

## Troubleshooting

### Debug menu doesn't open

- Make sure you're in-game (not main menu)
- Wait 15+ seconds after loading a save
- Press F1 first, then F8

### Game crashes on F3 (Time Freeze)

- The Time.set_timeScale RVA may have changed with a game update
- Check the log file for error messages

### Log Location

The mod creates `CairnDevUnlocker.log` in the game directory.
