using System.Diagnostics.CodeAnalysis;
using Windows.Win32;

namespace SmartDebugger;

public partial class DebugProcess
{
    public unsafe bool TryReadMemory(IntPtr address, nuint length, [NotNullWhen(true)] out byte[]? memory)
    {
        var buffer = new byte[length];
        nuint bytesRead = 0;

        fixed (void* bufferPtr = &buffer[0])
        {
            var result =
                PInvoke.ReadProcessMemory(_pi.hProcess, address.ToPointer(), bufferPtr, (uint)length, &bytesRead);

            if (!result || bytesRead < length)
            {
                memory = null;
                return false;
            }

            memory = buffer;
            return true;
        }
    }

    public unsafe bool TryWriteMemory(IntPtr address, Span<byte> memory)
    {
        var buffer = memory.ToArray();
        nuint bytesWritten = 0;

        fixed (void* bufferPtr = &buffer[0])
        {
            var result = PInvoke.WriteProcessMemory(_pi.hProcess, address.ToPointer(), bufferPtr, (uint)buffer.Length,
                &bytesWritten);
            return result && bytesWritten == (nuint)buffer.Length;
        }
    }
}