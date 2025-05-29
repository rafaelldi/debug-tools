using Grpc.Core;
using MonitorAgent;

namespace Monitor.Processes;

internal sealed class ProcessService : MonitorAgent.ProcessService.ProcessServiceBase
{
    public override Task<ProcessListResponse> GetProcessList(ProcessListRequest request,
        ServerCallContext context)
    {
        var processes = ProcessManager.GetProcessList();
        var response = new ProcessListResponse();
        response.Processes.AddRange(processes);
        return Task.FromResult(response);
    }

    public override Task<ProcessDetailsResponse> GetProcessDetails(ProcessDetailsRequest request,
        ServerCallContext context)
    {
        var processDetails = ProcessManager.GetProcessDetails(request.ProcessId);
        var response = new ProcessDetailsResponse
        {
            Details = processDetails
        };
        return Task.FromResult(response);
    }

    public override Task<ProcessEnvironmentResponse> GetProcessEnvironment(ProcessEnvironmentRequest request,
        ServerCallContext context)
    {
        var processEnvironment = ProcessManager.GetProcessEnvironment(request.ProcessId);
        var response = new ProcessEnvironmentResponse();
        response.Environment.AddRange(processEnvironment);
        return Task.FromResult(response);
    }
}