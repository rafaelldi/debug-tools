using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using Monitor.Common;

namespace Monitor.SessionConfigurations;

internal sealed class MetricsSessionConfiguration : SessionConfigurationWithInterval
{
    private const string SessionIdArgument = "SessionId";
    private const string MetricsArgument = "Metrics";
    private const string RefreshIntervalArgument = "RefreshInterval";
    private const string DefaultMeter = "System.Runtime";

    internal string Metrics { get; }
    internal override EventPipeProvider Provider { get; }

    internal MetricsSessionConfiguration(int processId, string? metrics, int? refreshInterval) :
        base(processId, refreshInterval)
    {
        Metrics = metrics ?? DefaultMeter;

        Provider = new EventPipeProvider(
            ProviderNames.SystemDiagnosticsMetrics,
            EventLevel.Informational,
            2L,
            new Dictionary<string, string>
            {
                [SessionIdArgument] = SessionId,
                [MetricsArgument] = Metrics,
                [RefreshIntervalArgument] = RefreshInterval.ToString()
            }
        );
    }
}