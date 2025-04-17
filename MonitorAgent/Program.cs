using MonitorAgent.Counters;
using MonitorAgent.Processes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddScoped<ProcessHandler>();
builder.Services.AddScoped<CounterHandler>();

var app = builder.Build();

app.MapGrpcService<ProcessManager>();
app.MapGrpcService<CounterManager>();

app.Run();