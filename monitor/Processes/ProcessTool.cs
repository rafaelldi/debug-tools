using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace Monitor.Processes;

[McpServerToolType]
internal sealed class ProcessTool
{
    [McpServerTool, Description("Get the process list of the current machine.")]
    internal static Task<string> GetProcessList(ILoggerFactory loggerFactory)
    {
        var processes = ProcessManager.GetProcessList();
        loggerFactory
            .CreateLogger("Monitor.Processes.ProcessTool")
            .LogDebug("Process list: {processes}", processes);

        var sb = new StringBuilder();
        foreach (var process in processes)
        {
            sb.AppendLine($"{process.ProcessId} - {process.ProcessName}");
        }
        var processString = sb.ToString();

        return Task.FromResult(processString);
    }
}