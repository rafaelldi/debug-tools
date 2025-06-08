using System.ComponentModel;
using ModelContextProtocol.Server;

namespace MonitorMcpServer.Processes;

[McpServerToolType]
internal sealed class ProcessTool
{
    [McpServerTool,
     Description("Get the process list of the current machine.")]
    internal static async Task<List<ProcessInfoDto>> GetProcessList(ProcessManager manager, CancellationToken token)
    {
        return await manager.GetProcessList(token);
    }

    [McpServerTool,
     Description("Find a process by its name.")]
    internal static async Task<ProcessInfoDto?> GetProcessByName(
        ProcessManager manager,
        [Description("The name  of the process to find.")]
        string processName,
        CancellationToken token)
    {
        return await manager.GetProcessByName(processName, token);
    }
}