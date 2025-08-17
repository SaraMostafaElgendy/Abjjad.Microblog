using System.Threading.Channels;

namespace Abjjad.Microblog.Background
{
    public class ImageProcessingQueue
    {
        private readonly Channel<int> _channel = Channel.CreateUnbounded<int>();
        public ValueTask QueuePostAsync(int postId) => _channel.Writer.WriteAsync(postId);
        public async Task<int> DequeueAsync(CancellationToken ct) => await _channel.Reader.ReadAsync(ct);
    }
}
