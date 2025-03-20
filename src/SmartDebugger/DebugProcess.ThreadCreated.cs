using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.System.Diagnostics.Debug;

namespace SmartDebugger;

public partial class DebugProcess
{
    private readonly Dictionary<uint, ProcessThread> _threadCache = new();

    /// <summary>
    /// Gets or sets a callback action that is invoked when a new thread is created 
    /// during the debugging process.
    /// </summary>
    /// <remarks>
    /// This property allows you to define custom logic to be executed whenever a
    /// thread is created and detected by the <see cref="DebugProcess"/> class. The
    /// callback provides the corresponding <see cref="ProcessThread"/> object for
    /// the newly created thread.
    /// </remarks>
    public Action<ProcessThread>? OnThreadCreated { get; set; }

    /// Handles the thread creation debug event by processing the newly created thread.
    /// It adds the thread information to the thread cache and invokes the registered
    /// callback function if available.
    /// <param name="threadId">The identifier of the created thread.</param>
    /// <param name="threadCreatedRecord">The unmanaged <c>CREATE_THREAD_DEBUG_INFO</c> structure
    /// containing information about the created thread.</param>
    private void HandleThreadCreated(uint threadId, CREATE_THREAD_DEBUG_INFO_unmanaged threadCreatedRecord)
    {
        if (OnThreadCreated is null)
            return;

        var threadId2 = (int)PInvoke.GetThreadId(threadCreatedRecord.hThread);
        var thread = _managedProcess.Threads.Cast<ProcessThread>()
            .Single(x => x.Id == threadId2);

        _threadCache[threadId] = thread;

        try
        {
            OnThreadCreated(thread);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
}