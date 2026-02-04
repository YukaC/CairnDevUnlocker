using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Doorstop
{
    /// <summary>
    /// Cairn Dev Unlocker v1.0.0
    /// Enables the hidden debug menu in Cairn
    /// 
    /// Hotkeys:
    ///   F8  - Open/close debug menu (game's native key)
    ///   F2  - Toggle cursor lock (keeps cursor visible)
    ///   F3  - Toggle time freeze (pauses game while using menu)
    ///   F1  - Toggle debug menu enable/disable
    /// </summary>
    public class Entrypoint
    {
        private const string VERSION = "1.0.0";
        
        private static string logPath;
        private static bool lastF1State = false;
        private static bool lastF2State = false;
        private static bool lastF3State = false;
        
        private static bool isDebugMenuEnabled = false;
        private static bool isCursorLocked = false;
        private static bool isTimeFrozen = false;
        
        private const int VK_F1 = 0x70;
        private const int VK_F2 = 0x71;
        private const int VK_F3 = 0x72;
        
        // DebugMenuUI offsets
        private const int UI_IS_ENABLED_OFFSET = 0x150;
        private const int UI_DATA_OFFSET = 0xE8;
        
        // DebugMenuData offsets
        private const int DATA_START_DISABLED = 0x19;
        
        // Unity Time.timeScale RVA (from dump.cs)
        private const long SET_TIME_SCALE_RVA = 0x3ADABF0;  // Time.set_timeScale
        
        private const long FIND_OBJECT_OF_TYPE_RVA = 0x3AD11E0;
        
        // Windows API
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        
        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);
        
        [DllImport("user32.dll")]
        private static extern IntPtr SetCursor(IntPtr hCursor);
        
        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        
        // IL2CPP Delegates
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr il2cpp_domain_get_delegate();
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr il2cpp_domain_get_assemblies_delegate(IntPtr domain, ref uint size);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr il2cpp_assembly_get_image_delegate(IntPtr assembly);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate IntPtr il2cpp_class_from_name_delegate(IntPtr image, string namespaze, string name);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr il2cpp_class_get_type_delegate(IntPtr klass);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr il2cpp_type_get_object_delegate(IntPtr type);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr il2cpp_thread_attach_delegate(IntPtr domain);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr FindObjectOfTypeDelegate(IntPtr systemType);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetTimeScaleDelegate(float scale);
        
        // Function pointers
        private static IntPtr gameAssemblyBase = IntPtr.Zero;
        private static il2cpp_domain_get_delegate il2cpp_domain_get;
        private static il2cpp_domain_get_assemblies_delegate il2cpp_domain_get_assemblies;
        private static il2cpp_assembly_get_image_delegate il2cpp_assembly_get_image;
        private static il2cpp_class_from_name_delegate il2cpp_class_from_name;
        private static il2cpp_class_get_type_delegate il2cpp_class_get_type;
        private static il2cpp_type_get_object_delegate il2cpp_type_get_object;
        private static il2cpp_thread_attach_delegate il2cpp_thread_attach;
        private static FindObjectOfTypeDelegate FindObjectOfType;
        private static SetTimeScaleDelegate SetTimeScale;
        
        private static IntPtr debugMenuUIClass = IntPtr.Zero;
        private static IntPtr uiInstance = IntPtr.Zero;
        
        public static void Start()
        {
            try
            {
                string gamePath = AppDomain.CurrentDomain.BaseDirectory;
                logPath = Path.Combine(gamePath, "CairnDevUnlocker.log");
                try { if (File.Exists(logPath)) File.Delete(logPath); } catch {}
                
                Log($"╔══════════════════════════════════════╗");
                Log($"║   Cairn Dev Unlocker v{VERSION}       ║");
                Log($"╠══════════════════════════════════════╣");
                Log($"║  F8 = Open/Close Debug Menu          ║");
                Log($"║  F2 = Toggle Cursor Lock             ║");
                Log($"║  F3 = Toggle Time Freeze             ║");
                Log($"║  F1 = Toggle Debug Menu Enable       ║");
                Log($"╚══════════════════════════════════════╝");
                
                Thread modThread = new Thread(ModLoop);
                modThread.IsBackground = true;
                modThread.Start();
            }
            catch (Exception ex)
            {
                Log($"Start Error: {ex.Message}");
            }
        }
        
        private static void ModLoop()
        {
            try
            {
                WaitForGameAssembly();
                if (!LoadFunctions()) return;
                
                Log("Waiting for game initialization...");
                Thread.Sleep(12000);
                
                IntPtr domain = il2cpp_domain_get();
                il2cpp_thread_attach(domain);
                
                FindClass();
                FindInstance();
                
                if (uiInstance == IntPtr.Zero) 
                { 
                    Log("ERROR: DebugMenuUI not found!");
                    Log("Make sure you're in-game (not main menu)");
                    return; 
                }
                
                // Auto-enable on startup
                EnableDebugMenu();
                
                Log("");
                Log("═══════════════════════════════════════");
                Log("  READY! Press F8 to open debug menu");
                Log("═══════════════════════════════════════");
                
                // Main loop
                while (true)
                {
                    HandleInput();
                    
                    // Keep cursor visible if locked
                    if (isCursorLocked)
                    {
                        ForceShowCursor();
                    }
                    
                    // Keep debug menu enabled
                    if (isDebugMenuEnabled)
                    {
                        try { Marshal.WriteByte(uiInstance, UI_IS_ENABLED_OFFSET, 1); } catch {}
                    }
                    
                    Thread.Sleep(16);
                }
            }
            catch (Exception ex)
            {
                Log($"FATAL: {ex.Message}");
            }
        }
        
        private static void HandleInput()
        {
            // F1 - Toggle debug menu enable
            bool f1 = (GetAsyncKeyState(VK_F1) & 0x8000) != 0;
            if (f1 && !lastF1State)
            {
                isDebugMenuEnabled = !isDebugMenuEnabled;
                if (isDebugMenuEnabled)
                    EnableDebugMenu();
                else
                    Log("[F1] Debug menu DISABLED");
            }
            lastF1State = f1;
            
            // F2 - Toggle cursor lock
            bool f2 = (GetAsyncKeyState(VK_F2) & 0x8000) != 0;
            if (f2 && !lastF2State)
            {
                isCursorLocked = !isCursorLocked;
                Log(isCursorLocked ? "[F2] Cursor LOCKED (always visible)" : "[F2] Cursor UNLOCKED (normal)");
                if (isCursorLocked) ForceShowCursor();
            }
            lastF2State = f2;
            
            // F3 - Toggle time freeze
            bool f3 = (GetAsyncKeyState(VK_F3) & 0x8000) != 0;
            if (f3 && !lastF3State)
            {
                isTimeFrozen = !isTimeFrozen;
                try
                {
                    SetTimeScale(isTimeFrozen ? 0.0f : 1.0f);
                    Log(isTimeFrozen ? "[F3] Time FROZEN (game paused)" : "[F3] Time RESUMED (game running)");
                }
                catch (Exception ex)
                {
                    Log($"[F3] Time freeze error: {ex.Message}");
                }
            }
            lastF3State = f3;
        }
        
        private static void ForceShowCursor()
        {
            try
            {
                ShowCursor(true);
                IntPtr cursor = LoadCursor(IntPtr.Zero, 32512); // IDC_ARROW
                SetCursor(cursor);
            }
            catch {}
        }
        
        private static void EnableDebugMenu()
        {
            try
            {
                // Set IsEnabled = true
                Marshal.WriteByte(uiInstance, UI_IS_ENABLED_OFFSET, 1);
                
                // Patch startDisabled = false
                IntPtr dataPtr = Marshal.ReadIntPtr(uiInstance, UI_DATA_OFFSET);
                if (dataPtr != IntPtr.Zero)
                {
                    Marshal.WriteByte(dataPtr, DATA_START_DISABLED, 0);
                }
                
                isDebugMenuEnabled = true;
                Log("[F1] Debug menu ENABLED!");
            }
            catch (Exception ex)
            {
                Log($"Enable error: {ex.Message}");
            }
        }
        
        private static void FindInstance()
        {
            if (debugMenuUIClass == IntPtr.Zero) return;
            try
            {
                IntPtr typePtr = il2cpp_class_get_type(debugMenuUIClass);
                IntPtr systemType = il2cpp_type_get_object(typePtr);
                uiInstance = FindObjectOfType(systemType);
                if (uiInstance != IntPtr.Zero)
                    Log("Found DebugMenuUI instance");
            }
            catch {}
        }
        
        private static void FindClass()
        {
            IntPtr domain = il2cpp_domain_get();
            uint count = 0;
            IntPtr assemblies = il2cpp_domain_get_assemblies(domain, ref count);
            
            for (uint i = 0; i < count; i++)
            {
                IntPtr asm = Marshal.ReadIntPtr(assemblies, (int)(i * IntPtr.Size));
                IntPtr img = il2cpp_assembly_get_image(asm);
                IntPtr cls = il2cpp_class_from_name(img, "TGBTools.DebugMenu", "DebugMenuUI");
                if (cls != IntPtr.Zero)
                {
                    debugMenuUIClass = cls;
                    Log("Found DebugMenuUI class");
                    return;
                }
            }
        }
        
        private static bool LoadFunctions()
        {
            try
            {
                il2cpp_domain_get = GetFunc<il2cpp_domain_get_delegate>("il2cpp_domain_get");
                il2cpp_domain_get_assemblies = GetFunc<il2cpp_domain_get_assemblies_delegate>("il2cpp_domain_get_assemblies");
                il2cpp_assembly_get_image = GetFunc<il2cpp_assembly_get_image_delegate>("il2cpp_assembly_get_image");
                il2cpp_class_from_name = GetFunc<il2cpp_class_from_name_delegate>("il2cpp_class_from_name");
                il2cpp_class_get_type = GetFunc<il2cpp_class_get_type_delegate>("il2cpp_class_get_type");
                il2cpp_type_get_object = GetFunc<il2cpp_type_get_object_delegate>("il2cpp_type_get_object");
                il2cpp_thread_attach = GetFunc<il2cpp_thread_attach_delegate>("il2cpp_thread_attach");
                
                // FindObjectOfType
                IntPtr findAddr = new IntPtr(gameAssemblyBase.ToInt64() + FIND_OBJECT_OF_TYPE_RVA);
                FindObjectOfType = Marshal.GetDelegateForFunctionPointer<FindObjectOfTypeDelegate>(findAddr);
                
                // Time.set_timeScale
                IntPtr timeAddr = new IntPtr(gameAssemblyBase.ToInt64() + SET_TIME_SCALE_RVA);
                SetTimeScale = Marshal.GetDelegateForFunctionPointer<SetTimeScaleDelegate>(timeAddr);
                
                Log("All functions loaded successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log($"LoadFunctions error: {ex.Message}");
                return false;
            }
        }
        
        private static T GetFunc<T>(string name) where T : Delegate
        {
            IntPtr ptr = GetProcAddress(gameAssemblyBase, name);
            if (ptr == IntPtr.Zero) throw new Exception($"Function not found: {name}");
            return Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }
        
        private static void WaitForGameAssembly()
        {
            while (gameAssemblyBase == IntPtr.Zero)
            {
                gameAssemblyBase = GetModuleHandle("GameAssembly.dll");
                Thread.Sleep(100);
            }
            Log("GameAssembly.dll loaded");
        }
        
        private static void Log(string msg)
        {
            try 
            { 
                File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss}] {msg}\n"); 
            } 
            catch {}
        }
    }
}
