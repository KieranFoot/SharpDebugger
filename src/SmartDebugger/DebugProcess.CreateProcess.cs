using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.System.Threading;
using Microsoft.Win32.SafeHandles;

namespace SmartDebugger;

public partial class DebugProcess
{
    /// <summary>
    /// Creates a new instance of <see cref="DebugProcess"/> for debugging the specified executable.
    /// </summary>
    /// <param name="executable">
    /// The path to the executable to be debugged.
    /// This must be a valid path to an executable file on the system.
    /// </param>
    /// <param name="arguments">
    /// An array of strings representing the arguments to pass to the executable.
    /// Each argument will be combined into a single command line for the process.
    /// </param>
    /// <param name="workingDirectory">
    /// The working directory for the process. If null, the current directory will be used.
    /// </param>
    /// <returns>
    /// A new instance of <see cref="DebugProcess"/> wrapping the created process.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the process fails to start.
    /// </exception>
    private static unsafe DebugProcess CreateProcess(string executable, string[] arguments,
        string? workingDirectory = null)
    {
        var si = new STARTUPINFOW();
        var pi = new PROCESS_INFORMATION();

        var flags = PROCESS_CREATION_FLAGS.DEBUG_ONLY_THIS_PROCESS;

        SECURITY_ATTRIBUTES? nullSecurityAttributes = null;

        BOOL inheritHandles = false;
        workingDirectory ??= Environment.CurrentDirectory;

        var chars = string.Join(" ", arguments).ToCharArray().ToList();
        chars.Add((char)0);
        var args = new Span<char>(chars.ToArray());

        var started = PInvoke.CreateProcess(
            executable,
            ref args,
            nullSecurityAttributes,
            nullSecurityAttributes,
            inheritHandles,
            flags,
            (void*)0,
            workingDirectory,
            in si,
            out pi);

        if (!started)
        {
            //TODO: Get Windows API error here.
            throw new InvalidOperationException("Process failed to start.");
        }

        var handle = new SafeProcessHandle(pi.hProcess, true);
        return new DebugProcess(executable, handle, pi);
    }
}