using MonitorAgent;

namespace MonitorMcpServer.Processes;

internal sealed class ProcessManager(ProcessService.ProcessServiceClient client)
{
    internal async Task<List<ProcessInfoDto>> GetProcessList(CancellationToken token)
    {
        var request = new ProcessListRequest();
        var response = await client.GetProcessListAsync(request, cancellationToken: token);

        return response.Processes
            .Select(it => new ProcessInfoDto(it.ProcessId, it.ProcessName))
            .ToList();
    }
}

internal record ProcessInfoDto(int ProcessId, string ProcessName);