using Monitor.Counters;
using Monitor.GC;
using Monitor.MemoryDumps;
using Monitor.Processes;
using Monitor.ThreadDumps;

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