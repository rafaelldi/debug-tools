using Grpc.Core;

namespace MonitorAgent.Counters;

internal sealed class CounterService(CounterManager counterManager) : MonitorAgent.CounterService.CounterServiceBase
{
    public override async Task GetCounterStream(CounterStreamRequest request,
        IServerStreamWriter<CounterValue> responseStream, ServerCallContext context)
    {
        try
        {
            var lifetimeDefinition = context.CancellationToken.ToLifetimeDefinition();
            var reader = counterManager.GetCounterStream(request, lifetimeDefinition.Lifetime);
            await foreach (var counterValue in reader.ReadAllAsync(context.CancellationToken))
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