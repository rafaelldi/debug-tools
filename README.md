A solution with tools to debug different problems in dotnet processes.

It consists of several projects:

- `DebugTools` - a project with multiple diagnostics tools aiming to help to debug various problems in a dotnet app. All
  tools are available through grpc endpoints.
- `ConsoleClient` - a console client to test grpc services;
- `DebugSamples` - a project with different problems, they can be triggered via http api.
