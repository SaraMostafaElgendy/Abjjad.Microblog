using Abjjad.Microblog.Data;
using Abjjad.Microblog.Services;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Abjjad.Microblog.Background
{
    public class ImageProcessingHostedService : BackgroundService
    {
        private readonly ImageProcessingQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IImageStorage _storage;
        private readonly int[] _sizes = new[] { 360, 720, 1080 };

        public ImageProcessingHostedService(ImageProcessingQueue queue, IServiceScopeFactory scopeFactory, IImageStorage storage)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _storage = storage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           
            while (!stoppingToken.IsCancellationRequested)
            {
                int postId;
                try { postId = await _queue.DequeueAsync(stoppingToken); }
                catch (OperationCanceledException) { break; }

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == postId, stoppingToken);
                    if (post == null || string.IsNullOrEmpty(post.OriginalImagePath)) continue;

                    var orig = await _storage.OpenReadAsync(post.OriginalImagePath);
                    if (orig == null) continue;

                    var processedPairs = new List<string>();
                    foreach (var w in _sizes)
                    {
                        orig.Seek(0, SeekOrigin.Begin);
                        using var image = await Image.LoadAsync(orig, stoppingToken);
                        var ratio = (double)w / image.Width;
                        var newHeight = (int)Math.Round(image.Height * ratio);
                        image.Mutate(x => x.Resize(w, newHeight));
                        using var ms = new MemoryStream();
                        var encoder = new WebpEncoder() { Quality = 75 };
                        await image.SaveAsync(ms, encoder, stoppingToken);
                        ms.Seek(0, SeekOrigin.Begin);
                        var saved = await _storage.SaveProcessedAsync(ms, $"{postId}_{w}.webp");
                        processedPairs.Add($"{w}:{saved}");
                    }

                    post.ProcessedImages = string.Join(',', processedPairs);
                    post.ImageProcessed = true;
                    db.Posts.Update(post);
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch
                {
                    // ignore for demo
                }
            }
        }
    }
}
