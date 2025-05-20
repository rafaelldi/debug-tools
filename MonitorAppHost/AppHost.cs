using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var monitorAgent =
    builder.AddProject<MonitorAgent>("monitor-agent");

var mcpServer =
    builder.AddProject<MonitorMcpServer>("mcp-server")
        .WithReference(monitorAgent)
        .WaitFor(monitorAgent);

var ollama =
    builder.AddOllama("ollama")
        .AddModel("llama3", "llama3.2:3b");

builder.Build().Run();