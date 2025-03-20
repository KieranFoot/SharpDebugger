using System.Diagnostics;
using Windows.Win32.System.Threading;
using Microsoft.Win32.SafeHandles;
using PeNet;

namespace SmartDebugger;

/// <summary>
/// Represents a process that can be debugged, providing functionality to manage and inspect the process execution,
/// load modules, handle events, and interact with the debugging environment.
/// </summary>
public partial class DebugProcess : IDisposable
{
    private readonly SafeProcessHandle _handle;
    private readonly PROCESS_INFORMATION _pi;
    private readonly Process _managedProcess;
    private readonly Task _debuggerTask;
    private readonly PeFile _peInfo;
    private bool _disposed;

    private DebugProcess(string executable, SafeProcessHandle handle, PROCESS_INFORMATION pi)
    {
        _handle = handle;
        _pi = pi;
        _managedProcess = Process.GetProcessById((int)pi.dwProcessId);
        _peInfo = new PeNet.PeFile(executable);
        _debuggerTask = DebuggerTask();
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _debuggerTask.Dispose();
            }
        }

        _disposed = true;
    }

    /// <InheritDoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}