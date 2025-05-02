using MonitorAgent.Counters;
using MonitorAgent.Processes;
using MonitorAgent.ThreadDumps;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddScoped<ProcessManager>();
builder.Services.AddScoped<ThreadDumpManager>();

var app = builder.Build();

app.MapGrpcService<ProcessService>();
app.MapGrpcService<CounterService>();
app.MapGrpcService<ThreadDumpService>();

app.Run();