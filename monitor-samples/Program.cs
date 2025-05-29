using MonitorSamples;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapDeadlockEndpoints();

app.Run();