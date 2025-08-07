A solution for monitoring different diagnostic information about dotnet processes.

It consists of several projects:

- `monitor` - a gRPC-based agent for collecting various diagnostic information about dotnet processes using
  `DiagnosticsClient`, the gRPC service is available at `http://localhost:5197`;
- `monitor-console-client` - a console client to test agent endpoints;
- `monitor-samples` - a sample project with different problems, they can be triggered via http api.
