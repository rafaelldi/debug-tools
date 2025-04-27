using System.Threading.Channels;
using Grpc.Core;

namespace MonitorAgent.Counters;

internal sealed class CounterService : MonitorAgent.CounterService.CounterServiceBase
{
    public override async Task GetCounterStream(CounterStreamRequest request,
        IServerStreamWriter<CounterValue> responseStream, ServerCallContext context)
    {
        try
        {
            var lifetimeDefinition = context.CancellationToken.ToLifetimeDefinition();
            var channel = Channel.CreateUnbounded<CounterValue>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
            });

            var sessionConfiguration = new CounterSessionConfiguration(request.ProcessId);
            var handler = new CounterSessionHandler(sessionConfiguration, channel.Writer);
            lifetimeDefinition.Lifetime.StartAttachedAsync(
                TaskScheduler.Default,
                async () => await handler.RunSession()
            );

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