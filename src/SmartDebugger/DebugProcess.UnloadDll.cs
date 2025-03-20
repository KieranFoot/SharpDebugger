using System.Diagnostics;
using Windows.Win32.System.Diagnostics.Debug;

namespace SmartDebugger;

public partial class DebugProcess
{
    public Action<ProcessModule>? OnModuleUnloaded { get; set; }

    private unsafe void HandleModuleUnloaded(UNLOAD_DLL_DEBUG_INFO unloadDllRecord)
    {
        var dllUnloadBase = new IntPtr(unloadDllRecord.lpBaseOfDll);
        if (_moduleCache.Remove(dllUnloadBase, out var unloadedModule))
        {
            if (OnModuleUnloaded is not null)
            {
                try
                {
                    OnModuleUnloaded(unloadedModule);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }
    }
}