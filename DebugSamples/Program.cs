using MonitorSamples.Deadlock;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithMetrics(metrics =>
    {
        metrics
            .AddRuntimeInstrumentation();
    });

var app = builder.Build();

app.MapDeadlockEndpoints();

app.Run();