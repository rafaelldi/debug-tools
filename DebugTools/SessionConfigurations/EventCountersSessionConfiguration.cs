using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using Monitor.Common;

namespace Monitor.SessionConfigurations;

internal sealed class EventCountersSessionConfiguration : AbstractSessionConfiguration
{
    private const string IntervalArgument = "EventCounterIntervalSec";
    private const int DefaultInterval = 1;

    internal int RefreshInterval { get; }
    internal override EventPipeProvider Provider { get; }

    internal EventCountersSessionConfiguration(int processId, int refreshInterval) : base(processId)
    {
        RefreshInterval = refreshInterval > 0 ? refreshInterval : DefaultInterval;
        Provider = new EventPipeProvider(
            ProviderNames.SystemRuntime,
            EventLevel.Informational,
            (long)EventKeywords.None,
            new Dictionary<string, string>
            {
                [IntervalArgument] = RefreshInterval.ToString()
            }
        );
    }
}