using MonitorAgent;
using MonitorMcpServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpcClient<ProcessService.ProcessServiceClient>(static it =>
{
    it.Address = new Uri("http://localhost:5197");
});
builder.Services.AddGrpcClient<ThreadDumpService.ThreadDumpServiceClient>(static it =>
{
    it.Address = new Uri("http://localhost:5197");
});

var app = builder.Build();

app.MapProcessEndpoint();
app.MapThreadDumpEndpoint();

app.Run();