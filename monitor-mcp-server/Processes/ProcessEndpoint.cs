namespace MonitorMcpServer.Processes;

internal static class ProcessEndpoint
{
    internal static void MapProcessEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapGet(
            "/processes",
            async (ProcessManager manager, CancellationToken token) =>
                await manager.GetProcessList(token)
        );
    }
}