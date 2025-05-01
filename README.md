A solution for monitoring different diagnostic information about dotnet processes.

It consists of several projects:

- `MonitorAgent` - a gRPC-based agent for collecting various diagnostic information about dotnet processes using
  `DiagnosticsClient`;
- `MonitorClient` - a console client to test agent endpoints;
- `MonitorMcpServer` - an MCP server that describes agent endpoints as MCP tools;
- `MonitorMcpClient` - an MCP client which uses Ollama and `MonitorMcpServer` to analyze diagnostic information.

# Run Ollama

To run Ollama, you could use the prepared `compose.yaml` script:

1. Run the `Compose` run configuration to start the Ollama container;
2. Run a model `docker exec -it ollama ollama run llama3.2:3b`.

# MCP Server

To test the MCP server, follow the steps

1. Run Ollama;
2. Run the `Servers` run configuration (`MonitorAgent` and `MonitorMcpServer`);
3. Run the `McpClient` run configuration.