using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

/// <summary>
/// Cairn Dev Unlocker - Activa el menu de debug con F1
/// Usa Unity Doorstop para inyección sin mod loader
/// </summary>
public static class Doorstop
{
    private static string logPath;
    private static bool isRunning = true;
    private static bool debugMenuEnabled = false;
    
    // Constantes para teclas
    private const int VK_F1 = 0x70;
    private const int VK_F2 = 0x71;
    
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    
    /// <summary>
    /// Punto de entrada de Doorstop
    /// </summary>
    public static void Main(string[] args)
    {
        try
        {
            // Configurar logging
            string gamePath = AppDomain.CurrentDomain.BaseDirectory;
            logPath = Path.Combine(gamePath, "CairnDevUnlocker.log");
            
            Log("=== Cairn Dev Unlocker Iniciado ===");
            Log($"Hora: {DateTime.Now}");
            Log($"Path: {gamePath}");
            Log($".NET Version: {Environment.Version}");
            
            // Iniciar el loop del mod en un thread separado
            Thread modThread = new Thread(ModLoop);
            modThread.IsBackground = true;
            modThread.Start();
            
            Log("Mod thread iniciado");
            Log("Presiona F1 para toggle debug menu");
            Log("Presiona F2 para desactivar el mod");
        }
        catch (Exception ex)
        {
            Log($"Error en inicialización: {ex.Message}");
            Log(ex.StackTrace ?? "");
        }
    }
    
    /// <summary>
    /// Loop principal del mod
    /// </summary>
    private static void ModLoop()
    {
        Log("ModLoop iniciado - esperando carga del juego...");
        
        // Esperar a que el juego cargue
        Thread.Sleep(8000);
        Log("Juego cargado, escuchando teclas...");
        
        while (isRunning)
        {
            try
            {
                // F1 para toggle debug menu
                if (IsKeyPressed(VK_F1))
                {
                    ToggleDebugMenu();
                    Thread.Sleep(300);
                }
                
                // F2 para salir
                if (IsKeyPressed(VK_F2))
                {
                    Log("F2 presionado - Desactivando mod");
                    isRunning = false;
                }
                
                Thread.Sleep(16);
            }
            catch (Exception ex)
            {
                Log($"Error en ModLoop: {ex.Message}");
                Thread.Sleep(1000);
            }
        }
        
        Log("ModLoop terminado");
    }
    
    private static bool IsKeyPressed(int vKey)
    {
        return (GetAsyncKeyState(vKey) & 0x8000) != 0;
    }
    
    /// <summary>
    /// Toggle del menu de debug usando Il2Cpp interop
    /// </summary>
    private static void ToggleDebugMenu()
    {
        debugMenuEnabled = !debugMenuEnabled;
        Log($"[F1] Debug Menu Toggle solicitado: {(debugMenuEnabled ? "ON" : "OFF")}");
        
        try
        {
            // NOTA: Para IL2CPP, necesitamos usar los pointers nativos
            // El método ToggleMenu está en RVA: 0x3251890
            // 
            // Esto requiere:
            // 1. Encontrar el objeto DebugMenuUI en escena
            // 2. Llamar al método usando su dirección nativa
            //
            // Por ahora, intentamos acceder via reflexión si los tipos están disponibles
            
            AccessDebugMenuViaReflection();
        }
        catch (Exception ex)
        {
            Log($"Error accediendo DebugMenuUI: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Intenta acceder al DebugMenuUI via reflexión
    /// </summary>
    private static void AccessDebugMenuViaReflection()
    {
        try
        {
            // Buscar el assembly del juego
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Log($"Assemblies cargados: {assemblies.Length}");
            
            foreach (var asm in assemblies)
            {
                if (asm.FullName?.Contains("TGBTools") == true || 
                    asm.FullName?.Contains("DebugMenu") == true)
                {
                    Log($"Encontrado assembly: {asm.FullName}");
                    
                    // Buscar el tipo DebugMenuUI
                    var debugMenuType = asm.GetType("TGBTools.DebugMenu.DebugMenuUI");
                    if (debugMenuType != null)
                    {
                        Log($"Encontrado tipo: {debugMenuType.FullName}");
                        
                        // Buscar método ToggleMenu
                        var toggleMethod = debugMenuType.GetMethod("ToggleMenu", 
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        
                        if (toggleMethod != null)
                        {
                            Log($"Encontrado método: {toggleMethod.Name}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log($"Reflexión falló: {ex.Message}");
        }
    }
    
    private static void Log(string message)
    {
        try
        {
            if (!string.IsNullOrEmpty(logPath))
            {
                File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss}] {message}\n");
            }
        }
        catch { }
    }
}
