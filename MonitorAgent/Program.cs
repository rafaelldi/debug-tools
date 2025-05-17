using MonitorAgent.Counters;
using MonitorAgent.GC;
using MonitorAgent.MemoryDumps;
using MonitorAgent.Processes;
using MonitorAgent.ThreadDumps;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<ProcessService>();
app.MapGrpcService<CounterService>();
app.MapGrpcService<ThreadDumpService>();
app.MapGrpcService<GCService>();
app.MapGrpcService<MemoryDumpService>();

app.Run();