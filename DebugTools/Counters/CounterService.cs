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

        var sessionLifetime = request.DurationInSeconds > 0
            ? sessionLifetimeDef.Lifetime.CreateTerminatedAfter(TimeSpan.FromSeconds(request.DurationInSeconds))
            : sessionLifetimeDef.Lifetime;

        var consumerTask = responseStream.WriteFromChannel(channel.Reader, sessionLifetime);

        var sessionConfiguration =
            new EventCountersSessionConfiguration(request.ProcessId, request.RefreshIntervalInSeconds);
        var sessionHandler = new CounterSessionHandler(sessionConfiguration, channel.Writer);
        var producerTask = sessionHandler.RunSession(sessionLifetime);

        await Task.WhenAll(producerTask, consumerTask);
    }
}