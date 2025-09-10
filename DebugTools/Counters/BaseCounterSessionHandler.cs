using System.Globalization;
using System.Threading.Channels;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Monitor.SessionConfigurations;
using MonitorAgent;
using static JetBrains.Lifetimes.Lifetime;

namespace Monitor.Counters;

internal abstract class BaseCounterSessionHandler<T>(
    T configuration,
    ChannelWriter<CounterValue> writer
) where T : BaseSessionConfiguration
{
    protected readonly T Configuration = configuration;
    protected readonly ChannelWriter<CounterValue> Writer = writer;

    internal async Task RunSession(Lifetime lifetime)
    {
        var client = new DiagnosticsClient(Configuration.ProcessId);

        var session = await client.StartEventPipeSessionAsync(
            Configuration.Provider,
            Configuration.RequestRundown,
            Configuration.CircularBufferMb,
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

        Writer.Complete();
    }

    protected abstract void HandleEvent(TraceEvent evt);

    protected static CounterValue MapToRateCounter(
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

    protected static CounterValue MapToMetricCounter(
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