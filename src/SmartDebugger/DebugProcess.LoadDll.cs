using System.Diagnostics;
using Windows.Win32.System.Diagnostics.Debug;

namespace SmartDebugger;

public partial class DebugProcess
{
    private readonly Dictionary<IntPtr, ProcessModule> _moduleCache = new();

    /// <summary>
    /// Gets or sets the callback action that is invoked when a module is loaded in the process being debugged.
    /// </summary>
    /// <remarks>
    /// The action is invoked with the loaded module as a <see cref="ProcessModule"/> parameter.
    /// This facilitates custom logic or actions to handle module loading events during a debugging session.
    /// </remarks>
    public Action<ProcessModule>? OnModuleLoaded { get; set; }

    /// <summary>
    /// Handles the event when a DLL module is loaded during the debugging process.
    /// Updates the module cache with the loaded module and invokes the module-loaded callback, if defined.
    /// </summary>
    /// <param name="loadDllRecord">The structure containing information about the loaded DLL.</param>
    private unsafe void HandleModuleLoaded(LOAD_DLL_DEBUG_INFO loadDllRecord)
    {
        var dllLoadBase = new IntPtr(loadDllRecord.lpBaseOfDll);
        var loadedModule = _managedProcess.Modules.Cast<ProcessModule>()
            .Single(x => x.BaseAddress == dllLoadBase);
        _moduleCache[dllLoadBase] = loadedModule;

        try
        {
            OnModuleLoaded?.Invoke(loadedModule);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
}