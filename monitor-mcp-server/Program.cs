using MonitorMcpServer;
using MonitorMcpServer.MemoryDumps;
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
builder.Services.AddScoped<MemoryDumpManager>();

var app = builder.Build();

app.MapMcp();

app.Run();