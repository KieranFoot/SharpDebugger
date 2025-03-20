using System.Diagnostics.CodeAnalysis;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.Debug;

namespace SmartDebugger;

public partial class DebugProcess
{
    /// <summary>
    /// Executes the main debugger loop for processing debug events in the debugged process.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation of the debugger loop. The task completes when
    /// the debugged process exits.
    /// </returns>
    private Task DebuggerTask()
    {
        var debugging = true;
        var debugStatus = NTSTATUS.DBG_CONTINUE;

        while (debugging)
        {
            if (!TryGetDebugEvent(out var debugEvent))
                continue;

            switch (debugEvent.Value.dwDebugEventCode)
            {
                case DEBUG_EVENT_CODE.CREATE_PROCESS_DEBUG_EVENT:
                    HandleProcessCreated();
                    break;

                case DEBUG_EVENT_CODE.EXIT_PROCESS_DEBUG_EVENT:
                    debugging = false;
                    break;

                case DEBUG_EVENT_CODE.CREATE_THREAD_DEBUG_EVENT:
                    HandleThreadCreated(debugEvent.Value.dwThreadId, debugEvent.Value.u.CreateThread);
                    break;

                case DEBUG_EVENT_CODE.EXIT_THREAD_DEBUG_EVENT:
                    HandleThreadExited(debugEvent.Value.dwThreadId, debugEvent.Value.u.ExitThread);
                    break;

                case DEBUG_EVENT_CODE.LOAD_DLL_DEBUG_EVENT:
                    HandleModuleLoaded(debugEvent.Value.u.LoadDll);
                    break;

                case DEBUG_EVENT_CODE.UNLOAD_DLL_DEBUG_EVENT:
                    HandleModuleUnloaded(debugEvent.Value.u.UnloadDll);
                    break;

                case DEBUG_EVENT_CODE.EXCEPTION_DEBUG_EVENT:
                    debugStatus = HandleException(debugEvent.Value.u.Exception);
                    break;

                case DEBUG_EVENT_CODE.OUTPUT_DEBUG_STRING_EVENT:
                    var debugStringRecord = debugEvent.Value.u.DebugString;
                    //TODO: Implement Debug string handling.
                    break;

                case DEBUG_EVENT_CODE.RIP_EVENT:
                    var ripRecord = debugEvent.Value.u.RipInfo;
                    //TODO: Implement RIP event handling.
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            PInvoke.ContinueDebugEvent(debugEvent.Value.dwProcessId, debugEvent.Value.dwThreadId, debugStatus);
            debugStatus = NTSTATUS.DBG_CONTINUE;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Attempts to retrieve the next debug event from the debugged process.
    /// </summary>
    /// <param name="debugEvent">
    /// When the method returns, contains a <see cref="DEBUG_EVENT"/> structure if the operation succeeds;
    /// otherwise, it is set to <c>null</c>.
    /// </param>
    /// <param name="timeout">
    /// The timeout duration in milliseconds to wait for a debug event. Defaults to 100 milliseconds.
    /// </param>
    /// <returns>
    /// <c>true</c> if a debug event is successfully retrieved; otherwise, <c>false</c>.
    /// </returns>
    private static bool TryGetDebugEvent([NotNullWhen(true)] out DEBUG_EVENT? debugEvent, uint timeout = 100)
    {
        var result = PInvoke.WaitForDebugEvent(out var evt, timeout);
        debugEvent = result ? evt : null;
        return result;
    }
}