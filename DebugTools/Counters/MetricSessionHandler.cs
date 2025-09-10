using System.Threading.Channels;
using Microsoft.Diagnostics.Tracing;
using Monitor.SessionConfigurations;
using MonitorAgent;

namespace Monitor.Counters;

internal sealed class MetricSessionHandler(
    MetricsSessionConfiguration configuration,
    ChannelWriter<CounterValue> writer
) : BaseCounterSessionHandler<MetricsSessionConfiguration>(configuration,  writer)
{
    protected override void HandleEvent(TraceEvent evt)
    {
        throw new NotImplementedException();
    }
}