using MonitorAgent;
using MonitorMcpServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpcClient<ProcessService.ProcessServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5197");
});
builder.Services.AddGrpcClient<CounterService.CounterServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5197");
});
builder.Services.AddGrpcClient<ThreadDumpService.ThreadDumpServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5197");
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