using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using static Monitor.Common.ProviderNames;

namespace Monitor.GC;

internal static class GCManager
{
    internal static async Task TriggerGC(int pid, CancellationToken ct)
    {
        var client = new DiagnosticsClient(pid);
        var providers = new List<EventPipeProvider>
        {
            new(
                MicrosoftWindowsDotNetRuntime,
                EventLevel.Informational,
                (long)ClrTraceEventParser.Keywords.GCHeapCollect,
                new Dictionary<string, string>()
            )
        };

        using (var session = await client.StartEventPipeSessionAsync(providers, requestRundown: false, token: ct))
        {
            using (var source = new EventPipeEventSource(session.EventStream))
            {
                var processingTask = Task.Run(() =>
                {
                    try
                    {
                        source.Process();
                    }
                    catch (Exception ex)
                    {
                    }
                });

                var stoppingTask = Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    session.Stop();
                });

                await Task.WhenAll(processingTask, stoppingTask);
            }
        }
    }
}