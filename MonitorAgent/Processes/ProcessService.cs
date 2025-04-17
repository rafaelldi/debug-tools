using Grpc.Core;

namespace MonitorAgent.Processes;

internal sealed class ProcessService(ProcessManager processManager) : MonitorAgent.ProcessService.ProcessServiceBase
{
    public override Task<ProcessListResponse> GetProcessList(ProcessListRequest request, ServerCallContext context)
    {
        var processes = processManager.GetProcessList();
        var response = new ProcessListResponse();
        response.Processes.AddRange(processes);
        return Task.FromResult(response);
    }

    public override Task<ProcessDetailsResponse> GetProcessDetails(ProcessDetailsRequest request,
        ServerCallContext context)
    {
        var processDetails = processManager.GetProcessDetails(request.ProcessId);
        var response = new ProcessDetailsResponse
        {
            Details = processDetails
        };
        return Task.FromResult(response);
    }

    public override Task<ProcessEnvironmentResponse> GetProcessEnvironment(ProcessEnvironmentRequest request,
        ServerCallContext context)
    {
        var processEnvironment = processManager.GetProcessEnvironment(request.ProcessId);
        var response = new ProcessEnvironmentResponse();
        response.Environment.AddRange(processEnvironment);
        return Task.FromResult(response);
    }
}