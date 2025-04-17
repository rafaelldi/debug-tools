using Grpc.Core;

namespace MonitorAgent.Counters;

internal sealed class CounterService(CounterManager counterManager) : MonitorAgent.CounterService.CounterServiceBase
{
    public override async Task GetCounterStream(CounterStreamRequest request,
        IServerStreamWriter<CounterValue> responseStream, ServerCallContext context)
    {
        try
        {
            var reader = counterManager.GetCounterStream(request, context.CancellationToken);
            await foreach (var counterValue in reader.ReadAllAsync(context.CancellationToken))
            {
                await responseStream.WriteAsync(counterValue);
            }
        }
        catch (OperationCanceledException)
        {
            //do nothing
        }
    }
}