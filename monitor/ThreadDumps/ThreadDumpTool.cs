using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Monitor.ThreadDumps;

[McpServerToolType]
internal sealed class ThreadDumpTool
{
    [McpServerTool, Description("Capture the process thread dump for the requested process.")]
    internal static async Task<string> GetThreadDump(
        ILoggerFactory loggerFactory,
        [Description("The process id to capture a thread dump. It should be an integer number.")]
        string processId,
        CancellationToken token)
    {
        if (!int.TryParse(processId, out var pid)) return "";

        var dump = await ThreadDumpManager.CollectThreadDump(pid, token);
        loggerFactory
            .CreateLogger("Monitor.ThreadDumps.ThreadDumpTool")
            .LogDebug("Thread dump captured for process {processId}", processId);

        return dump;
    }
}