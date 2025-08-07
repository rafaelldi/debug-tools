using MonitorSamples.Deadlock;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapDeadlockEndpoints();

app.Run();