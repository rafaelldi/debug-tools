using System.Threading.Channels;
using Grpc.Core;
using JetBrains.Lifetimes;

namespace Monitor.Common;

internal static class ServerStreamWriterExtensions
{
    internal static async Task WriteFromChannel<T>(this IServerStreamWriter<T> streamWriter, ChannelReader<T> reader,
        Lifetime lifetime)
    {
        try
        {
            await foreach (var item in reader.ReadAllAsync(lifetime))
            {
                await streamWriter.WriteAsync(item, lifetime);
            }
        }
        catch (OperationCanceledException)
        {
            //Operation was cancelled, do nothing
        }
    }
}