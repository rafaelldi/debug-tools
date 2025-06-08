using MonitorAgent;

namespace MonitorMcpServer;

internal static class GrpcClientRegistration
{
    private const string MonitorAddress = "http://localhost:5197";
    private static readonly Uri MonitorUri = new(MonitorAddress);

    internal static void AddGrpcClients(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpcClient<ProcessService.ProcessServiceClient>(static it => { it.Address = MonitorUri; });
        builder.Services.AddGrpcClient<ThreadDumpService.ThreadDumpServiceClient>(static it =>
        {
            it.Address = MonitorUri;
        });
        builder.Services.AddGrpcClient<MemoryDumpService.MemoryDumpServiceClient>(static it =>
        {
            it.Address = MonitorUri;
        });
    }
}