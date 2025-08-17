using Abjjad.Microblog.Data;
using Abjjad.Microblog.Models;
using Abjjad.Microblog.Background;
using Microsoft.EntityFrameworkCore;

namespace Abjjad.Microblog.Services
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _db;
        private readonly IImageStorage _storage;
        private readonly ImageProcessingQueue _queue;
        public PostService(AppDbContext db, IImageStorage storage, ImageProcessingQueue queue)
        {
            _db = db;
            _storage = storage;
            _queue = queue;
        }

        public async Task<Post> CreatePostAsync(int authorId, string text, Stream? imageStream, string? fileName, string? contentType)
        {
            var post = new Post
            {
                AuthorId = authorId,
                Text = text.Length <= 140 ? text : text.Substring(0,140),
                CreatedAt = DateTime.UtcNow,
                Latitude = RandomCoordinate(-90,90),
                Longitude = RandomCoordinate(-180,180)
            };

            if (imageStream != null && !string.IsNullOrEmpty(fileName))
            {
                var savedName = await _storage.SaveOriginalAsync(imageStream, fileName, contentType ?? "image/jpeg");
                post.OriginalImagePath = savedName;
                post.ImageProcessed = false;
            }

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(post.OriginalImagePath))
            {
                await _queue.QueuePostAsync(post.Id);
            }

            return post;
        }

        public async Task<Post?> GetPostAsync(int id) =>
            await _db.Posts.Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Post>> GetTimelineAsync(int page = 1, int pageSize = 20)
        {
            return await _db.Posts.Include(p => p.Author)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync();
        }

        private static double RandomCoordinate(double min, double max)
        {
            return Random.Shared.NextDouble() * (max - min) + min;
        }
    }
}
