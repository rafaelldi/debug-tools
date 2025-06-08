using Grpc.Core;
using MonitorAgent;

namespace Monitor.MemoryDumps;

internal sealed class MemoryDumpService : MonitorAgent.MemoryDumpService.MemoryDumpServiceBase
{
    public override async Task<MemoryDumpResponse> CollectMemoryDump(MemoryDumpRequest request,
        ServerCallContext context)
    {
        var dumpId = await MemoryDumpManager.CollectMemoryDump(request.ProcessId, request.DumpType,
            context.CancellationToken);

        return new MemoryDumpResponse
        {
            MemoryDumpId = dumpId
        };
    }
}