using MonitorAgent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpcClient<ProcessService.ProcessServiceClient>(static it =>
{
    it.Address = new Uri("http://localhost:5197");
});
builder.Services.AddGrpcClient<CounterService.CounterServiceClient>(static it =>
{
    it.Address = new Uri("http://localhost:5197");
});
builder.Services.AddGrpcClient<ThreadDumpService.ThreadDumpServiceClient>(static it =>
{
    it.Address = new Uri("http://localhost:5197");
});

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp();

app.Run();