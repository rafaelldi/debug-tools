using System.Diagnostics;
using Grpc.Core;
using Grpc.Net.Client;
using MonitorClient;

using var channel = GrpcChannel.ForAddress("http://localhost:5197");

var client = new ProcessService.ProcessServiceClient(channel);

var processes = await client.GetProcessListAsync(new ProcessListRequest());
Console.WriteLine("Available processes:");
foreach (var process in processes.Processes)
{
    Console.WriteLine($"{process.ProcessId} - {process.ProcessName}");
}

Console.WriteLine();

Console.WriteLine("Enter process id to get counters:");
var processId = Console.ReadLine();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    // ReSharper disable once AccessToDisposedClosure
    cts.Cancel();
};

var counterClient = new CounterService.CounterServiceClient(channel);
Debug.Assert(processId != null, nameof(processId) + " != null");
using var counterCall = counterClient.GetCounterStream(new CounterStreamRequest { ProcessId = int.Parse(processId) });

try
{
    await foreach (var counter in counterCall.ResponseStream.ReadAllAsync(cts.Token))
    {
        Console.WriteLine(
            $"{counter.Timestamp} - {counter.Name} - {counter.DisplayName} - {counter.ProviderName} - {counter.Value}");
    }
}
catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
{
    Console.WriteLine("Operation canceled");
}