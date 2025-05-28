A solution for monitoring different diagnostic information about dotnet processes.

It consists of several projects:

- `MonitorAgent` - a gRPC-based agent for collecting various diagnostic information about dotnet processes using
  `DiagnosticsClient`, the gRPC service is available at `http://localhost:5197`;
- `MonitorClient` - a console client to test agent endpoints;
- `MonitorMcpServer` - an MCP server that describes gRPC agent endpoints as MCP tools, it is available at
  `http://localhost:5213`;
- `MonitorSamples` - a sample project with different problems, they can be triggered via http api.