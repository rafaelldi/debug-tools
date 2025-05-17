using Grpc.Core;

namespace MonitorAgent.ThreadDumps;

internal sealed class ThreadDumpService : MonitorAgent.ThreadDumpService.ThreadDumpServiceBase
{
    public override async Task<ThreadDumpResponse> CollectThreadDump(ThreadDumpRequest request,
        ServerCallContext context)
    {
        var dump = await ThreadDumpManager.CollectThreadDump(request.ProcessId, context.CancellationToken);

        return new ThreadDumpResponse
        {
            Content = dump
        };
    }
}