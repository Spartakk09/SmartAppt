using System.Threading.Channels;

namespace SmartAppt.Common.Logging;

internal static class LogQueue
{
    private static readonly Channel<string> _channel =
        Channel.CreateBounded<string>(
            new BoundedChannelOptions(100_000)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropOldest
            });

    public static void Enqueue(string line)
    {
        _channel.Writer.TryWrite(line);
    }

    public static IAsyncEnumerable<string> ReadAllAsync(
        CancellationToken token)
        => _channel.Reader.ReadAllAsync(token);
}
