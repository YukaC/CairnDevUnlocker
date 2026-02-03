# Documentación Técnica - Cairn Dev Unlocker

## Análisis del Juego

### Información General

| Propiedad      | Valor  |
| -------------- | ------ |
| Motor          | Unity  |
| Runtime        | IL2CPP |
| Versión IL2CPP | 31     |
| Arquitectura   | x64    |

### Archivos Clave

- `GameAssembly.dll` - Código IL2CPP compilado (~92MB)
- `global-metadata.dat` - Metadata para Il2CppDumper (~23MB)

## Extracción de Metadata

Usamos Il2CppDumper v6.7.46:

```powershell
.\Il2CppDumper.exe "GameAssembly.dll" "global-metadata.dat" "output"
```

### Archivos Generados

| Archivo       | Descripción                          |
| ------------- | ------------------------------------ |
| `dump.cs`     | Todas las clases decompiladas (49MB) |
| `DummyDll/`   | 143 DLLs stub para referencia        |
| `script.json` | Metadata en formato JSON             |
| `il2cpp.h`    | Headers C++                          |

## APIs Identificadas

### DebugMenuUI

```csharp
// Namespace: TGBTools.DebugMenu
// Assembly: TheGameBakers.TGBTools.DebugMenu.Runtime.dll

public class DebugMenuUI : MonoBehaviour
{
    // Eventos
    internal event Action OnOpening;
    internal event Action OnOpened;
    internal event Action OnClosing;
    internal event Action OnClosed;

    // Propiedades
    public GameObject Canvas { get; }
    internal bool IsEnabled { get; set; }  // RVA: 0x2B19D70
    internal bool IsOpened { get; }        // RVA: 0x32520B0

    // Métodos principales
    internal void ToggleMenu();            // RVA: 0x3251890
    private void Open();                   // RVA: 0x3250260
    private void Close();                  // RVA: 0x324CA20
}
```

### Sistema de Comandos

```csharp
// Clase base para comandos de debug
public abstract class DebugMenuCommand
{
    public virtual bool IsAvailable();
    public virtual string GetDisplayName();
    public abstract ExecuteResult OnExecute(bool shortcut);
}

// Tipos de comandos
- DebugMenuCommandSimple    // Ejecución simple
- DebugMenuCommandToggle    // Toggle on/off
- DebugMenuCommandOptions   // Múltiples opciones
- DebugMenuCommandParams    // Con parámetros
```

## Unity Doorstop

### Funcionamiento

1. El juego carga `winhttp.dll` (sistema de Windows)
2. Doorstop intercepta esta carga con un proxy DLL
3. Lee `doorstop_config.ini` para obtener el DLL objetivo
4. Inicia CoreCLR y ejecuta nuestro `Main()`
5. Nuestro código corre en un thread separado

### Configuración

```ini
[General]
enabled=true
target_assembly=CairnDevUnlocker.dll
redirect_output_log=true

[Il2Cpp]
coreclr_path=    # Auto-detectado de .NET instalado
corlib_dir=      # Auto-detectado de .NET instalado
```

## Limitaciones

### IL2CPP vs Mono

En juegos Mono, podemos acceder directamente a tipos del juego via reflexión. En IL2CPP:

- El código C# está compilado a C++ nativo
- Los tipos originales no existen en runtime
- Se necesita Il2CppInterop para marshalling

### Solución Actual

El mod actual usa `GetAsyncKeyState` de Windows para detectar F1, y registra eventos. Para acceso completo a `DebugMenuUI.ToggleMenu()`, se necesitaría:

1. Il2CppInterop.Runtime para interop
2. Bindings generados del juego
3. Llamadas via punteros nativos

## Referencias

- [Unity Doorstop](https://github.com/NeighTools/UnityDoorstop)
- [Il2CppDumper](https://github.com/Perfare/Il2CppDumper)
- [Il2CppInterop](https://github.com/BepInEx/Il2CppInterop)
