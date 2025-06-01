using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;
using MonitorAgent;

namespace MonitorMcpServer;

[McpServerToolType]
internal sealed class ProcessTool
{
    [McpServerTool,
     Description(
         "Get the process list of the current machine. Each list element consists of process id and process name.")]
    internal static async Task<string> GetProcessList(
        ProcessService.ProcessServiceClient client,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var request = new ProcessListRequest();
        var response = await client.GetProcessListAsync(request, cancellationToken: cancellationToken);
        var sb = new StringBuilder();
        foreach (var process in response.Processes)
        {
            sb.AppendLine($"{process.ProcessId} - {process.ProcessName}");
        }

        var processes = sb.ToString();

        loggerFactory
            .CreateLogger("MonitorMcpServer.ProcessTool")
            .LogDebug("Process list: {processes}", processes);

        return processes;
    }
}