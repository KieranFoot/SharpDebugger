using System.Diagnostics;

namespace SmartDebugger;

public partial class DebugProcess
{
    public Action<Process>? OnProcessCreated { get; set; }

    private void HandleProcessCreated()
    {
        if (OnProcessCreated is null)
            return;

        try
        {
            OnProcessCreated.Invoke(_managedProcess);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
}