using MonitorAgent;

namespace MonitorMcpServer;

internal static class ProcessEndpoint
{
    internal static void MapProcessEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/processes",
            async (ProcessService.ProcessServiceClient client, CancellationToken token) =>
            {
                var request = new ProcessListRequest();
                var response = await client.GetProcessListAsync(request, cancellationToken: token);
                return response.Processes
                    .Select(it => new ProcessInfoDto(it.ProcessId, it.ProcessName))
                    .ToList();
            });
    }
}

internal record ProcessInfoDto(int ProcessId, string ProcessName);