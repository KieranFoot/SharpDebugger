using System.Diagnostics.CodeAnalysis;
using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.Debug;

namespace SmartDebugger;

public partial class DebugProcess
{
    /// <summary>
    /// Gets or sets the callback action that is invoked when a breakpoint
    /// is hit during debugging.
    /// </summary>
    /// <remarks>
    /// The action receives the address of the breakpoint as a parameter, represented
    /// as an <see cref="IntPtr"/>. This allows for custom handling or processing
    /// whenever the debugger encounters a breakpoint.
    /// </remarks>
    public Action<IntPtr>? OnBreakpointHit { get; set; }

    /// <summary>
    /// Handles breakpoint exceptions that occur during the debugging process.
    /// </summary>
    /// <param name="exception">
    /// Information about the breakpoint exception, including the exception record and exception address within the debugged process.
    /// </param>
    /// <returns>
    /// Returns an <c>NTSTATUS</c> code indicating the outcome of handling the breakpoint exception.
    /// It returns <c>DBG_EXCEPTION_HANDLED</c> as this is not a "real" exception.
    /// </returns>
    private unsafe NTSTATUS HandleBreakpoint(EXCEPTION_DEBUG_INFO exception)
    {
        if (OnBreakpointHit is not null)
        {
            var breakpointAddress = new IntPtr(exception.ExceptionRecord.ExceptionAddress);
            OnBreakpointHit(breakpointAddress);
        }

        return NTSTATUS.DBG_EXCEPTION_HANDLED;
    }

    private bool TrySetBreakpoint(IntPtr address, [NotNullWhen(true)] out IDisposable? breakpoint)
    {
        if (!TryReadMemory(address, 1, out var original))
        {
            breakpoint = null;
            return false;
        }

        if (!TryWriteMemory(address, new byte[] { 0xCC }))
        {
            breakpoint = null;
            return false;
        }

        breakpoint = new DisposableBreakpoint(address, original[0], this);
        return true;
    }

    private class DisposableBreakpoint(IntPtr address, byte original, DebugProcess process) : IDisposable
    {
        public void Dispose()
        {
            _ = process.TryWriteMemory(address, new byte[] { original });
        }
    }
}