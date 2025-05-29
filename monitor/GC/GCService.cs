using Grpc.Core;

namespace MonitorAgent.GC;

internal sealed class GCService : MonitorAgent.GCService.GCServiceBase
{
    public override async Task<TriggerGCResponse> TriggerGC(TriggerGCRequest request, ServerCallContext context)
    {
        await GCManager.TriggerGC(request.ProcessId, context.CancellationToken);
        return new TriggerGCResponse();
    }
}