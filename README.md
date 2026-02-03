# Cairn Dev Unlocker

🎮 **Mod para activar el menú de debug del juego Cairn presionando F1**

> Sin necesidad de MelonLoader ni BepInEx - Solo tirar archivos en la carpeta del juego

![.NET 6.0](https://img.shields.io/badge/.NET-6.0-512BD4?logo=dotnet)
![Unity Doorstop](https://img.shields.io/badge/Unity-Doorstop-000000?logo=unity)
![License](https://img.shields.io/badge/license-MIT-green)

## ✨ Características

- **Toggle del Debug Menu** con la tecla F1
- **Sin mod loader** - Usa Unity Doorstop para inyección directa
- **Ligero** - Solo ~35KB en total
- **Fácil de desinstalar** - Solo borrar 3 archivos

## 📦 Instalación

1. Descarga la última versión de [Releases](../../releases)
2. Copia los 3 archivos a la carpeta del juego:
   - `winhttp.dll`
   - `doorstop_config.ini`
   - `CairnDevUnlocker.dll`
3. ¡Listo! Ejecuta el juego normalmente

```
📁 Cairn/
├── Cairn.exe
├── winhttp.dll          ← Copiar aquí
├── doorstop_config.ini  ← Copiar aquí
├── CairnDevUnlocker.dll ← Copiar aquí
└── ... (otros archivos)
```

## 🎮 Uso

| Tecla  | Acción                   |
| ------ | ------------------------ |
| **F1** | Toggle del menú de debug |
| **F2** | Desactivar el mod        |

## 🔧 Compilación

Requiere [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

```powershell
cd src
dotnet build -c Release
```

El DLL compilado estará en `src/bin/Release/net6.0/`

## 🛠️ Cómo funciona

### Tecnologías usadas

- **Unity Doorstop** - Inyector de DLL para juegos Unity (proxy DLL)
- **Il2CppDumper** - Extracción de metadata del juego IL2CPP
- **.NET 6.0** - Runtime para el mod

### Proceso de ingeniería inversa

1. **Análisis del juego**: Cairn usa Unity IL2CPP (código compilado a C++ nativo)
2. **Extracción de metadata**: Usamos Il2CppDumper para extraer las clases del juego
3. **Identificación de APIs**: Encontramos `TGBTools.DebugMenu.DebugMenuUI.ToggleMenu()`
4. **Inyección**: Unity Doorstop carga nuestro DLL al iniciar el juego
5. **Detección de input**: Usamos `GetAsyncKeyState` de Windows para detectar F1

### Clase objetivo del juego

```csharp
// Namespace: TGBTools.DebugMenu
public class DebugMenuUI : MonoBehaviour
{
    internal void ToggleMenu();     // RVA: 0x3251890
    internal bool IsEnabled { get; set; }
    internal bool IsOpened { get; }
}
```

## 📁 Estructura del proyecto

```
CairnDevUnlocker/
├── src/
│   ├── CairnDevUnlocker.cs     # Código fuente principal
│   └── CairnDevUnlocker.csproj # Proyecto .NET 6
├── release/
│   ├── winhttp.dll             # Doorstop proxy
│   ├── doorstop_config.ini     # Configuración
│   └── CairnDevUnlocker.dll    # Mod compilado
├── docs/
│   └── TECHNICAL.md            # Documentación técnica
├── README.md
└── LICENSE
```

## 🗑️ Desinstalación

Elimina estos archivos de la carpeta del juego:

- `winhttp.dll`
- `doorstop_config.ini`
- `CairnDevUnlocker.dll`
- `CairnDevUnlocker.log` (si existe)

## 📝 Logs

El mod crea `CairnDevUnlocker.log` en la carpeta del juego con información de debug.

## ⚠️ Notas

- Este mod es para uso educacional y personal
- Probado con la versión actual de Cairn
- Si el juego se actualiza, puede ser necesario regenerar los bindings

## 📄 Licencia

MIT License - Ver [LICENSE](LICENSE)

## 🙏 Créditos

- [Unity Doorstop](https://github.com/NeighTools/UnityDoorstop) - Inyector de DLL
- [Il2CppDumper](https://github.com/Perfare/Il2CppDumper) - Extractor de metadata
- [The Game Bakers](https://thegamebakers.com/) - Desarrolladores de Cairn
