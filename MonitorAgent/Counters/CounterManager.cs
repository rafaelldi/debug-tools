using System.Threading.Channels;
using JetBrains.Lifetimes;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;

namespace MonitorAgent.Counters;

internal sealed class CounterManager
{
    internal ChannelReader<CounterValue> GetCounterStream(CounterStreamRequest request, Lifetime lifetime)
    {
        var channel = Channel.CreateUnbounded<CounterValue>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        });

        WriteCounterValue(request.ProcessId, channel.Writer, lifetime);

        return channel.Reader;
    }

    private async Task WriteCounterValue(int pid, ChannelWriter<CounterValue> channel, Lifetime lifetime)
    {
        var configuration = new CounterSessionConfiguration();
        var client = new DiagnosticsClient(pid);

        var session = await client.StartEventPipeSessionAsync(
            configuration.Provider,
            configuration.RequestRundown,
            configuration.CircularBufferMb,
            lifetime
        );
        lifetime.AddDispose(session);

        var source = new EventPipeEventSource(session.EventStream);
        lifetime.AddDispose(source);

        lifetime.Bracket(
            () => source.Dynamic.All += HandleEvent,
            () => source.Dynamic.All -= HandleEvent
        );
    }

    private void HandleEvent(TraceEvent evt)
    {
    }
}