namespace MonitorMcpServer.ThreadDumps;

internal static class ThreadDumpEndpoint
{
    internal static void MapThreadDumpEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapGet(
            "/processes/{pid:int}/thread-dump",
            async (ThreadDumpManager manager, CancellationToken token, int pid) =>
                await manager.CollectThreadDump(pid, token));
    }
}