using System.Threading.Channels;
using Grpc.Core;

namespace Monitor.Common;

internal static class ServerStreamWriterExtensions
{
    internal static async Task WriteFromChannel<T>(this IServerStreamWriter<T> streamWriter, ChannelReader<T> reader,
        CancellationToken token)
    {
        try
        {
            await foreach (var item in reader.ReadAllAsync(token))
            {
                await streamWriter.WriteAsync(item, token);
            }
        }
        catch (OperationCanceledException)
        {
            //Operation was cancelled, do nothing
        }
    }
}