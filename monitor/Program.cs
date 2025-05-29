using Monitor.Processes;
using Monitor.ThreadDumps;
using MonitorAgent.Counters;
using MonitorAgent.GC;
using MonitorAgent.MemoryDumps;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithHttpTransport()
    .WithTools<ProcessTool>()
    .WithTools<ThreadDumpTool>();

var app = builder.Build();

app.MapGrpcService<ProcessService>();
app.MapGrpcService<CounterService>();
app.MapGrpcService<ThreadDumpService>();
app.MapGrpcService<GCService>();
app.MapGrpcService<MemoryDumpService>();
app.MapMcp();

app.Run();