using MonitorAgent;

namespace MonitorMcpServer.MemoryDumps;

internal sealed class MemoryDumpManager(MemoryDumpService.MemoryDumpServiceClient client)
{
    internal async Task<MemoryDumpIdDto> CollectMemoryDump(int pid, CancellationToken token)
    {
        var request = new MemoryDumpRequest
        {
            ProcessId = pid,
            DumpType = MemoryDumpType.Full
        };
        var response = await client.CollectMemoryDumpAsync(request, cancellationToken: token);

        return new MemoryDumpIdDto(response.MemoryDumpId);
    }
}

internal record MemoryDumpIdDto(string Id);