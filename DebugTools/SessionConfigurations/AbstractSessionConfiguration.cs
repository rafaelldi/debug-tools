using Microsoft.Diagnostics.NETCore.Client;

namespace Monitor.SessionConfigurations;

internal abstract class AbstractSessionConfiguration(int processId)
{
    private const int DefaultCircularBufferMb = 256;

    internal string SessionId { get; } = Guid.NewGuid().ToString();
    internal int ProcessId { get; } = processId;
    internal bool RequestRundown => false;
    internal int CircularBufferMb => DefaultCircularBufferMb;
    internal abstract EventPipeProvider Provider { get; }
}