using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using Monitor.Common;

namespace Monitor.SessionConfigurations;

internal sealed class EventCountersSessionConfiguration(int processId) : AbstractSessionConfiguration(processId)
{
    private const string IntervalArgument = "EventCounterIntervalSec";
    private const int DefaultInterval = 1;

    internal int RefreshInterval => DefaultInterval;

    internal override EventPipeProvider Provider { get; } = new(
        ProviderNames.SystemRuntime,
        EventLevel.Informational,
        (long)EventKeywords.None,
        new Dictionary<string, string>
        {
            [IntervalArgument] = DefaultInterval.ToString()
        }
    );
}