using Grpc.Net.Client;
using MonitorAgent;

const string processName = "monitor-samples";

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    // ReSharper disable once AccessToDisposedClosure
    cts.Cancel();
};

using var channel = GrpcChannel.ForAddress("http://localhost:5197");

var client = new ProcessService.ProcessServiceClient(channel);
var process = await client.GetProcessByNameAsync(new ProcessByNameRequest { ProcessName = processName },
    cancellationToken: cts.Token);
Console.WriteLine(process.ToString());

if (process.Process is null) return;

var threadDumpClient = new ThreadDumpService.ThreadDumpServiceClient(channel);
var threadDump =
    await threadDumpClient.CollectThreadDumpAsync(new ThreadDumpRequest { ProcessId = process.Process.ProcessId },
        cancellationToken: cts.Token);
Console.WriteLine(threadDump.ToString());