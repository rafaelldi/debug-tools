using System.Threading.Channels;
using Grpc.Core;
using JetBrains.Lifetimes;
using Monitor.Common;
using Monitor.SessionConfigurations;
using MonitorAgent;

namespace Monitor.Counters;

internal sealed class CounterService : MonitorAgent.CounterService.CounterServiceBase
{
    public override async Task GetCounterStream(CounterStreamRequest request,
        IServerStreamWriter<CounterValue> responseStream, ServerCallContext context)
    {
        using var lifetimeDef = new LifetimeDefinition();

        var sessionLifetimeDef = lifetimeDef.Lifetime.CreateNested();
        context.CancellationToken.Register(() => sessionLifetimeDef.Terminate());

        var channel = Channel.CreateUnbounded<CounterValue>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        });

        int? duration = request.HasDurationInSeconds ? request.DurationInSeconds : null;
        int? refreshInterval = request.HasRefreshIntervalInSeconds ? request.RefreshIntervalInSeconds : null;
        var providerName = request.HasProviderName ? request.ProviderName : null;

        var sessionLifetime = duration > 0
            ? sessionLifetimeDef.Lifetime.CreateTerminatedAfter(TimeSpan.FromSeconds(duration.Value))
            : sessionLifetimeDef.Lifetime;

        var consumerTask = responseStream.WriteFromChannel(channel.Reader, sessionLifetime);

        var sessionConfiguration =
            new EventCountersSessionConfiguration(request.ProcessId, providerName, refreshInterval);
        var sessionHandler = new CounterSessionHandler(sessionConfiguration, channel.Writer);
        var producerTask = sessionHandler.RunSession(sessionLifetime);

        await Task.WhenAll(producerTask, consumerTask);
    }

    public override async Task GetMetricStream(MetricStreamRequest request,
        IServerStreamWriter<CounterValue> responseStream, ServerCallContext context)
    {
        using var lifetimeDef = new LifetimeDefinition();

        var sessionLifetimeDef = lifetimeDef.Lifetime.CreateNested();
        context.CancellationToken.Register(() => sessionLifetimeDef.Terminate());

        var channel = Channel.CreateUnbounded<CounterValue>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        });

        int? duration = request.HasDurationInSeconds ? request.DurationInSeconds : null;
        int? refreshInterval = request.HasRefreshIntervalInSeconds ? request.RefreshIntervalInSeconds : null;
        var meterName = request.HasMeterName ? request.MeterName : null;

        var sessionLifetime = duration > 0
            ? sessionLifetimeDef.Lifetime.CreateTerminatedAfter(TimeSpan.FromSeconds(duration.Value))
            : sessionLifetimeDef.Lifetime;

        var consumerTask = responseStream.WriteFromChannel(channel.Reader, sessionLifetime);

        var sessionConfiguration =
            new MetricsSessionConfiguration(request.ProcessId, meterName, refreshInterval);

        await Task.WhenAll(consumerTask);
    }
}