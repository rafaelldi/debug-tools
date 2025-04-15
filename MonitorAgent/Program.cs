using MonitorAgent.ProcessList;
using MonitorAgent.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddScoped<ProcessHandler>();

var app = builder.Build();

app.MapGrpcService<AgentService>();

app.Run();