using System.Threading.Channels;
using Microsoft.Diagnostics.Tracing;
using Monitor.SessionConfigurations;
using MonitorAgent;

namespace Monitor.Counters;

internal sealed class CounterSessionHandler(
    EventCountersSessionConfiguration configuration,
    ChannelWriter<CounterValue> writer
) : BaseCounterSessionHandler<EventCountersSessionConfiguration>(configuration, writer)
{
    private const string EventName = "EventCounters";

    protected override void HandleEvent(TraceEvent evt)
    {
        if (evt.ProcessID != Configuration.ProcessId) return;
        if (evt.EventName != EventName) return;
        if (evt.ProviderName != Configuration.ProviderName) return;

        var payloadVal = (IDictionary<string, object>)evt.PayloadValue(0);
        var payloadFields = (IDictionary<string, object>)payloadVal["Payload"];

        var name = payloadFields["Name"].ToString();
        if (name is null) return;

        var counter = MapCounterEvent(
            evt.ProviderName,
            name,
            evt.TimeStamp,
            Configuration.RefreshInterval,
            payloadFields
        );

        Writer.TryWrite(counter);
    }

    private static CounterValue MapCounterEvent(
        string providerName,
        string name,
        DateTime timeStamp,
        int refreshInterval,
        IDictionary<string, object> payloadFields)
    {
        var displayName = payloadFields["DisplayName"].ToString();
        displayName = string.IsNullOrEmpty(displayName) ? name : displayName;
        var displayUnits = payloadFields["DisplayUnits"].ToString();

        if (payloadFields["CounterType"].ToString() == "Sum")
        {
            var value = (double)payloadFields["Increment"];
            return MapToRateCounter(timeStamp, displayName, displayUnits, providerName, value, null, refreshInterval);
        }
        else
        {
            var value = (double)payloadFields["Mean"];
            return MapToMetricCounter(timeStamp, displayName, displayUnits, providerName, value, null);
        }
    }
}