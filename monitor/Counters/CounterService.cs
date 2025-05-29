using System.Threading.Channels;
using Grpc.Core;
using Monitor.Common;
using Monitor.SessionConfigurations;
using MonitorAgent;

namespace Monitor.Counters;

internal sealed class CounterService : MonitorAgent.CounterService.CounterServiceBase
{
    public override async Task GetCounterStream(CounterStreamRequest request,
        IServerStreamWriter<CounterValue> responseStream, ServerCallContext context)
    {
        var channel = Channel.CreateUnbounded<CounterValue>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        });

        var sessionConfiguration = new EventCountersSessionConfiguration(request.ProcessId);
        var handler = new CounterSessionHandler(sessionConfiguration, channel.Writer);
        var duration = TimeSpan.FromSeconds(request.DurationInSeconds);
        var producerTask = handler.RunSession(duration, context.CancellationToken);

        var consumerTask = responseStream.WriteFromChannel(channel.Reader, context.CancellationToken);

        await Task.WhenAll(producerTask, consumerTask);
    }
}