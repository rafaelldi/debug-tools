using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using Monitor.Common;

namespace Monitor.SessionConfigurations;

internal sealed class EventCountersSessionConfiguration : AbstractSessionConfiguration
{
    private const string IntervalArgument = "EventCounterIntervalSec";
    private const int DefaultInterval = 1;

    internal string ProviderName { get; }
    internal int RefreshInterval { get; }
    internal override EventPipeProvider Provider { get; }

    internal EventCountersSessionConfiguration(int processId, string? provider, int? refreshInterval) : base(processId)
    {
        ProviderName = provider ?? ProviderNames.SystemRuntime;
        RefreshInterval = refreshInterval > 0 ? refreshInterval.Value : DefaultInterval;

        Provider = new EventPipeProvider(
            ProviderName,
            EventLevel.Informational,
            (long)EventKeywords.None,
            new Dictionary<string, string>
            {
                [IntervalArgument] = RefreshInterval.ToString()
            }
        );
    }
}