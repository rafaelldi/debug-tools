using System.Threading.Channels;
using Grpc.Core;
using Monitor.SessionConfigurations;
using MonitorAgent;

namespace Monitor.Counters;

internal sealed class CounterService : MonitorAgent.CounterService.CounterServiceBase
{
    public override async Task GetCounterStream(CounterStreamRequest request,
        IServerStreamWriter<CounterValue> responseStream, ServerCallContext context)
    {
        try
        {
            var channel = Channel.CreateUnbounded<CounterValue>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
            });

            var sessionConfiguration = new EventCountersSessionConfiguration(request.ProcessId);
            var handler = new CounterSessionHandler(sessionConfiguration, channel.Writer);
            // lifetimeDefinition.Lifetime.StartAttachedAsync(
            //     TaskScheduler.Default,
            //     async () => await handler.RunSession(context.CancellationToken)
            // );

            await foreach (var counterValue in channel.Reader.ReadAllAsync(context.CancellationToken))
            {
                await responseStream.WriteAsync(counterValue, context.CancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            //do nothing
        }
    }
}