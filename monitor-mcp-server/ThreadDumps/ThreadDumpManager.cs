using MonitorAgent;

namespace MonitorMcpServer.ThreadDumps;

internal sealed class ThreadDumpManager(ThreadDumpService.ThreadDumpServiceClient client)
{
    internal async Task<ThreadDumpDto> CollectThreadDump(int pid, CancellationToken token)
    {
        var request = new ThreadDumpRequest
        {
            ProcessId = pid
        };
        var response = await client.CollectThreadDumpAsync(request, cancellationToken: token);

        return Map(response.Dump);
    }

    private static ThreadDumpDto Map(ThreadDump threadDump)
    {
        var threads = threadDump.Treads.Select(thread =>
            new ThreadDto(
                thread.ThreadId,
                thread.Frames.Select(it => it).ToList()
            )
        ).ToList();

        return new ThreadDumpDto(threads);
    }
}

internal record ThreadDumpDto(List<ThreadDto> Threads);

internal record ThreadDto(string ThreadId, List<string> Frames);