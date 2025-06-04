using System.ComponentModel;
using ModelContextProtocol.Server;

namespace MonitorMcpServer.Processes;

[McpServerToolType]
internal sealed class ProcessTool
{
    [McpServerTool,
     Description("Get the process list of the current machine")]
    internal static async Task<List<ProcessInfoDto>> GetProcessList(ProcessManager manager, CancellationToken token)
    {
        return await manager.GetProcessList(token);
    }
}