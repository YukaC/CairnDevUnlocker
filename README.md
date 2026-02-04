# Cairn Dev Unlocker

Enables the hidden developer debug menu in **Cairn** (The Game Bakers, 2025).

## Features

- 🎮 **Unlocks the Debug Menu** - Access hidden developer tools
- ⏸️ **Time Freeze** - Pause the game while navigating the menu (F3)
- 🖱️ **Cursor Lock** - Keep cursor visible at all times (F2)

## Installation

1. Download the latest release from [Releases](https://github.com/YukaChka/CairnDevUnlocker/releases)
2. Extract the contents to your Cairn game folder:
   - `CairnDevUnlocker.dll` → game folder
   - `doorstop_config.ini` → game folder
   - `winhttp.dll` → game folder (Doorstop loader)
3. Launch the game

## Usage

| Key    | Action                                           |
| ------ | ------------------------------------------------ |
| **F8** | Open/Close Debug Menu (game's native key)        |
| **F3** | Toggle Time Freeze (pause game while using menu) |
| **F2** | Toggle Cursor Lock (keeps cursor visible)        |
| **F1** | Toggle Debug Menu Enable/Disable                 |

### Recommended Workflow

1. Load into a game save
2. Press **F3** to freeze time (optional but recommended)
3. Press **F8** to open the debug menu
4. Use the menu as needed
5. Press **F8** to close, then **F3** to unfreeze time

## Uninstallation

Delete these files from your Cairn game folder:

- `CairnDevUnlocker.dll`
- `doorstop_config.ini`
- `winhttp.dll`
- `CairnDevUnlocker.log` (if exists)

## Technical Details

This mod uses [Unity Doorstop](https://github.com/NeighTools/UnityDoorstop) to inject into the game's IL2CPP runtime. It patches:

- `DebugMenuUI.IsEnabled` - Enables the debug menu
- `DebugMenuData.startDisabled` - Prevents the menu from auto-disabling

See [docs/TECHNICAL.md](docs/TECHNICAL.md) for more details.

## Compatibility

- ✅ Cairn (Steam version, 2025)
- ⚠️ May not work with future game updates

## License

MIT License - see [LICENSE](LICENSE)

## Credits

- [Unity Doorstop](https://github.com/NeighTools/UnityDoorstop) - DLL injection
