using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;
using MonitorAgent;

namespace MonitorMcpServer;

[McpServerToolType]
internal sealed class ProcessTool
{
    [McpServerTool, Description("Get the process list of the current machine.")]
    internal static async Task<string> GetProcessList(
        ProcessService.ProcessServiceClient client,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var response = await client.GetProcessListAsync(new ProcessListRequest(), cancellationToken: cancellationToken);
        var sb = new StringBuilder();
        foreach (var process in response.Processes)
        {
            sb.AppendLine($"{process.ProcessId} - {process.ProcessName}");
        }

        var processes = sb.ToString();

        loggerFactory
            .CreateLogger("MonitorMcpServer.ProcessTool")
            .LogInformation("Process list: {processes}", processes);

        return processes;
    }
}