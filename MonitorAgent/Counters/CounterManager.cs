using Grpc.Core;

namespace MonitorAgent.Counters;

internal sealed class CounterManager(CounterHandler counterHandler) : MonitorAgent.CounterManager.CounterManagerBase
{
    public override async Task GetCounterStream(CounterStreamRequest request,
        IServerStreamWriter<CounterValue> responseStream, ServerCallContext context)
    {
        try
        {
            var reader = counterHandler.GetCounterStream(request, context.CancellationToken);
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