using System.ComponentModel;
using ModelContextProtocol.Server;

namespace MonitorMcpServer.MemoryDumps;

[McpServerToolType]
internal sealed class MemoryDumpTool
{
    [McpServerTool,
     Description("Capture the memory dump for the requested process.")]
    internal static async Task<MemoryDumpIdDto?> CollectMemoryDump(
        MemoryDumpManager manager,
        [Description("The process id to capture a memory dump. It should be an integer number.")]
        string processId,
        CancellationToken token)
    {
        if (!int.TryParse(processId, out var pid)) return null;

        return await manager.CollectMemoryDump(pid, token);
    }
}