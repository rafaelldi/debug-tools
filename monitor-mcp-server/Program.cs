using MonitorMcpServer;
using MonitorMcpServer.Processes;
using MonitorMcpServer.ThreadDumps;

var builder = WebApplication.CreateBuilder(args);

builder.AddGrpcClients();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddScoped<ProcessManager>();
builder.Services.AddScoped<ThreadDumpManager>();

var app = builder.Build();

app.MapProcessEndpoint();
app.MapThreadDumpEndpoint();
app.MapMcp("mcp");

app.Run();