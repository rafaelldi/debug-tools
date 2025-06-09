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

    public override Task<DeleteMemoryDumpResponse> DeleteMemoryDump(DeleteMemoryDumpRequest request,
        ServerCallContext context)
    {
        MemoryDumpManager.DeleteMemoryDump(request.MemoryDumpId);
        return Task.FromResult(new DeleteMemoryDumpResponse());
    }

    public override Task<AnalyzeClrStackResponse> AnalyzeClrStack(AnalyzeClrStackRequest request,
        ServerCallContext context)
    {
        MemoryDumpManager.GetClrStack(request.MemoryDumpId);
        return Task.FromResult(new AnalyzeClrStackResponse());
    }
}