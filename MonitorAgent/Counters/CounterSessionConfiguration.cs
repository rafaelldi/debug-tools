using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using MonitorAgent.Common;

namespace MonitorAgent.Counters;

internal sealed class CounterSessionConfiguration
{
    private const string IntervalArgument = "EventCounterIntervalSec";
    private const string DefaultInterval = "1";

    internal string SessionId { get; } = Guid.NewGuid().ToString();

    internal EventPipeProvider Provider { get; } = new(
        Providers.SystemRuntimeProvider,
        EventLevel.Informational,
        (long)EventKeywords.None,
        new Dictionary<string, string>
        {
            [IntervalArgument] = DefaultInterval
        }
    );

    internal bool RequestRundown { get; } = false;

    internal int CircularBufferMb { get; } = 256;
}