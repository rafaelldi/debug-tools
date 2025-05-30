namespace Monitor.Processes;

internal static class ProcessEndpoint
{
    internal static void MapProcessEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/processes", () =>
        {
            var processes = ProcessManager.GetProcessList()
                .Select(it => new ProcessInfoDto(it.ProcessId, it.ProcessName));
            return processes;
        });
    }
}

internal class ProcessInfoDto(int ProcessId, string ProcessName);