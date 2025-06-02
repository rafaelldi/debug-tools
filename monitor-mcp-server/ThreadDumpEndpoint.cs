using MonitorAgent;

namespace MonitorMcpServer;

internal static class ThreadDumpEndpoint
{
    internal static void MapThreadDumpEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/processes/{pid:int}/thread-dump",
            async (ThreadDumpService.ThreadDumpServiceClient client, CancellationToken token, int pid) =>
            {
                var request = new ThreadDumpRequest
                {
                    ProcessId = pid
                };
                var response = await client.CollectThreadDumpAsync(request, cancellationToken: token);
                return response.Content;
            });
    }
}