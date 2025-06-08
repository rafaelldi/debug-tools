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

    internal async Task<ProcessInfoDto?> GetProcessByName(string processName, CancellationToken token)
    {
        var request = new ProcessByNameRequest { ProcessName = processName };
        var response = await client.GetProcessByNameAsync(request, cancellationToken: token);
        var process = response.Process;
        return process is not null ? new ProcessInfoDto(process.ProcessId, process.ProcessName) : null;
    }
}

internal record ProcessInfoDto(int ProcessId, string ProcessName);