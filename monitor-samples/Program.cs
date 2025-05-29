using MonitorSamples.Sample1;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapSample1Endpoints();

app.Run();