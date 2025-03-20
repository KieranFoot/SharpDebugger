using System.Diagnostics;
using Windows.Win32.System.Diagnostics.Debug;

namespace SmartDebugger;

public partial class DebugProcess
{
    public Action<ProcessThread>? OnThreadExited { get; set; }

    private void HandleThreadExited(uint threadId, EXIT_THREAD_DEBUG_INFO exitThreadRecord)
    {
        if (_threadCache.Remove(threadId, out var thread))
        {
            if (OnThreadExited is not null)
            {
                try
                {
                    OnThreadExited(thread);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }
    }
}