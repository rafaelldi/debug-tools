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
                return Map(response.Dump);
            });
    }

    private static ThreadDumpDto Map(ThreadDump threadDump)
    {
        var threads = threadDump.Treads.Select(thread =>
            new ThreadDto(
                thread.Id,
                thread.Frames.Select(it => new FrameDto(it.Name)).ToList()
            )
        ).ToList();

        return new ThreadDumpDto(threads);
    }
}

internal record ThreadDumpDto(List<ThreadDto> Threads);

internal record ThreadDto(string Id, List<FrameDto> Frames);

internal record FrameDto(string Name);