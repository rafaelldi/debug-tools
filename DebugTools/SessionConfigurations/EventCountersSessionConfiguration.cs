using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using Monitor.Common;

namespace Monitor.SessionConfigurations;

internal sealed class EventCountersSessionConfiguration : SessionConfigurationWithInterval
{
    private const string IntervalArgument = "EventCounterIntervalSec";

    internal string ProviderName { get; }
    internal override EventPipeProvider Provider { get; }

    internal EventCountersSessionConfiguration(int processId, string? provider, int? refreshInterval) :
        base(processId, refreshInterval)
    {
        ProviderName = provider ?? ProviderNames.SystemRuntime;

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