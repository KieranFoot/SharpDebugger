using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.Debug;

namespace SmartDebugger;

public partial class DebugProcess
{
    /// <summary>
    /// Handles exception debug events occurring in the debugged process.
    /// </summary>
    /// <param name="exception">
    /// Information about the exception that occurred, including the exception record and context.
    /// </param>
    /// <returns>
    /// Returns an <c>NTSTATUS</c> code indicating how the exception has been handled within the debugging process.
    /// For example, it might return <c>DBG_EXCEPTION_HANDLED</c> if a breakpoint exception occurred and was handled,
    /// or <c>DBG_EXCEPTION_NOT_HANDLED</c> if the exception was not handled.
    /// </returns>
    private NTSTATUS HandleException(EXCEPTION_DEBUG_INFO exception)
    {
        if (exception.ExceptionRecord.ExceptionCode == NTSTATUS.STATUS_BREAKPOINT)
            return HandleBreakpoint(exception);

        return NTSTATUS.DBG_EXCEPTION_NOT_HANDLED;
    }
}