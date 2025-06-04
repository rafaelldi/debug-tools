using System.ComponentModel;
using ModelContextProtocol.Server;

namespace MonitorMcpServer.ThreadDumps;

[McpServerToolType]
internal sealed class ThreadDumpTool
{
    [McpServerTool,
     Description("Capture the process thread dump for the requested process.")]
    internal static async Task<ThreadDumpDto?> GetThreadDump(
        ThreadDumpManager manager,
        [Description("The process id to capture a thread dump. It should be an integer number.")]
        string processId,
        CancellationToken token)
    {
        if (!int.TryParse(processId, out var pid)) return null;

        return await manager.CollectThreadDump(pid, token);
    }
}