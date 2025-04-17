using System.Threading.Channels;

namespace MonitorAgent.Counters;

internal sealed class CounterManager
{
    internal ChannelReader<CounterValue> GetCounterStream(CounterStreamRequest request,
        CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<CounterValue>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        });

        return channel.Reader;
    }
}