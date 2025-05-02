using Grpc.Core;

namespace MonitorAgent.ThreadDumps;

internal sealed class ThreadDumpService(ThreadDumpManager manager)
    : MonitorAgent.ThreadDumpService.ThreadDumpServiceBase
{
    public override async Task<ThreadDumpResponse> GetThreadDump(ThreadDumpRequest request, ServerCallContext context)
    {
        var dump = await manager.GetThreadDump(request.ProcessId, context.CancellationToken);
        var response = new ThreadDumpResponse
        {
            Content = dump
        };
        return response;
    }
}