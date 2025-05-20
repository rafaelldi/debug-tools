using MonitorAgent;
using MonitorMcpServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpcClient<ProcessService.ProcessServiceClient>(static it =>
{
    it.Address = new Uri("http://monitor-agent");
});
builder.Services.AddGrpcClient<CounterService.CounterServiceClient>(static it =>
{
    it.Address = new Uri("http://monitor-agent");
});
builder.Services.AddGrpcClient<ThreadDumpService.ThreadDumpServiceClient>(static it =>
{
    it.Address = new Uri("http://monitor-agent");
});

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithStdioServerTransport()
    .WithTools<ProcessTool>()
    .WithTools<ThreadDumpTool>();

var app = builder.Build();

app.MapMcp();

app.Run();