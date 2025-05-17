using System.ComponentModel;
using ModelContextProtocol.Server;
using MonitorAgent;

namespace MonitorMcpServer;

[McpServerToolType]
internal sealed class ThreadDumpTool
{
    [McpServerTool, Description("Capture the process thread dump for the requested process.")]
    internal static async Task<string> GetThreadDump(
        ThreadDumpService.ThreadDumpServiceClient client,
        ILoggerFactory loggerFactory,
        [Description("The process id to capture a thread dump. It should be an integer number.")]
        string processId,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(processId, out var pid)) return "";

        var request = new ThreadDumpRequest
        {
            ProcessId = pid
        };
        var response = await client.CollectThreadDumpAsync(request, cancellationToken: cancellationToken);
        var dump = response.Content;

        loggerFactory
            .CreateLogger("MonitorMcpServer.ThreadDumpTool")
            .LogInformation("Thread dump captured for process {processId}", processId);

        return dump;
    }
}