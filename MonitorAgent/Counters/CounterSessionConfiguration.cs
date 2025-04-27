using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using MonitorAgent.Common;

namespace MonitorAgent.Counters;

internal sealed class CounterSessionConfiguration
{
    private const string IntervalArgument = "EventCounterIntervalSec";
    private const int DefaultInterval = 1;
    private const int DefaultCircularBufferMb = 256;

    internal string SessionId { get; } = Guid.NewGuid().ToString();
    internal int RefreshInterval => DefaultInterval;
    internal bool RequestRundown => false;
    internal int CircularBufferMb => DefaultCircularBufferMb;
    internal int ProcessId { get; }
    internal EventPipeProvider Provider { get; }

    public CounterSessionConfiguration(int processId)
    {
        ProcessId = processId;
        Provider = new EventPipeProvider(
            Providers.SystemRuntimeProvider,
            EventLevel.Informational,
            (long)EventKeywords.None,
            new Dictionary<string, string>
            {
                [IntervalArgument] = RefreshInterval.ToString()
            }
        );
    }
}