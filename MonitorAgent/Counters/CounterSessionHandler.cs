using System.Globalization;
using System.Threading.Channels;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using static JetBrains.Lifetimes.Lifetime;

namespace MonitorAgent.Counters;

internal sealed class CounterSessionHandler
{
    private const string EventName = "EventCounters";

    private readonly CounterSessionConfiguration _configuration;
    private readonly ChannelWriter<CounterValue> _writer;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CounterSessionHandler(CounterSessionConfiguration configuration, ChannelWriter<CounterValue> writer)
    {
        _configuration = configuration;
        _writer = writer;
    }

    internal async Task RunSession()
    {
        var lifetime = AsyncLocal.Value;

        lifetime.OnTermination(() => _writer.Complete());

        var client = new DiagnosticsClient(_configuration.ProcessId);

        var session = await client.StartEventPipeSessionAsync(
            _configuration.Provider,
            _configuration.RequestRundown,
            _configuration.CircularBufferMb,
            lifetime
        );
        lifetime.AddDispose(session);

        var source = new EventPipeEventSource(session.EventStream);
        lifetime.AddDispose(source);

        lifetime.Bracket(
            () => source.Dynamic.All += HandleEvent,
            () => source.Dynamic.All -= HandleEvent
        );

        var processingTask = lifetime.StartAttached(
            TaskScheduler.Default,
            () => source.Process()
        );

        var stoppingTask = lifetime.StartAttachedAsync(
            TaskScheduler.Default,
            async () => await WaitToCancellationAndStopSession(session)
        );

        await Task.WhenAll(processingTask, stoppingTask);
    }

    private void HandleEvent(TraceEvent evt)
    {
        if (evt.ProcessID != _configuration.ProcessId) return;

        if (evt.EventName != EventName) return;

        var payloadVal = (IDictionary<string, object>)evt.PayloadValue(0);
        var payloadFields = (IDictionary<string, object>)payloadVal["Payload"];

        var name = payloadFields["Name"].ToString();
        if (name is null) return;

        var counter = MapCounterEvent(
            evt.ProviderName,
            name,
            evt.TimeStamp,
            _configuration.RefreshInterval,
            payloadFields
        );

        _writer.TryWrite(counter);
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

    private static CounterValue MapToRateCounter(
        DateTime timestamp,
        string name,
        string? units,
        string providerName,
        double value,
        string? tags,
        int refreshInterval)
    {
        var displayUnits = string.IsNullOrEmpty(units) ? "Count" : units;
        var counter = new CounterValue
        {
            Timestamp = timestamp.ToString(CultureInfo.CurrentCulture),
            Name = name,
            DisplayName = $"{name} ({displayUnits} / {refreshInterval} sec)",
            ProviderName = providerName,
            Value = Math.Round(value, 2),
            Type = CounterType.Rate
        };

        if (tags is not null)
        {
            counter.Tags = tags;
        }

        return counter;
    }

    private static CounterValue MapToMetricCounter(
        DateTime timestamp,
        string name,
        string? units,
        string providerName,
        double value,
        string? tags)
    {
        var counter = new CounterValue
        {
            Timestamp = timestamp.ToString(CultureInfo.CurrentCulture),
            Name = name,
            DisplayName = string.IsNullOrEmpty(units) ? name : $"{name} ({units})",
            ProviderName = providerName,
            Value = Math.Round(value, 2),
            Type = CounterType.Metric
        };

        if (tags is not null)
        {
            counter.Tags = tags;
        }

        return counter;
    }

    private static async Task WaitToCancellationAndStopSession(EventPipeSession session)
    {
        try
        {
            await Task.Delay(-1, AsyncLocal.Value.ToCancellationToken());
        }
        catch (TaskCanceledException)
        {
            //do nothing
        }

        await UsingAsync(async lifetime => await StopSession(session, lifetime));
    }

    private static async Task StopSession(EventPipeSession session, Lifetime lifetime)
    {
        try
        {
            await session.StopAsync(lifetime.CreateTerminatedAfter(TimeSpan.FromSeconds(30)));
        }
        catch (EndOfStreamException)
        {
        }
        catch (TimeoutException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }
        catch (ServerNotAvailableException)
        {
        }
    }
}