using Abjjad.Microblog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Abjjad.Microblog.Controllers
{
    [Authorize]
    public class TimelineController : Controller
    {
        private readonly IPostService _posts;
        private readonly IImageStorage _storage;

        public TimelineController(IPostService posts, IImageStorage storage)
        {
            _posts = posts;
            _storage = storage;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var posts = await _posts.GetTimelineAsync(page);
            var model = posts.Select(p => new TimelineViewModel
            {
                Id = p.Id,
                Text = p.Text,
                Username = p.Author?.Username ?? "unknown",
                CreatedAt = p.CreatedAt,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                OriginalImageUrl = p.OriginalImagePath != null ? _storage.GetPublicUrl(p.OriginalImagePath) : null,
                ProcessedImages = ParseProcessedImages(p.ProcessedImages),
                ImageProcessed = p.ImageProcessed
            }).ToList();
            return View(model);
        }

        private static Dictionary<int, string> ParseProcessedImages(string? processed)
        {
            var dict = new Dictionary<int, string>();
            if (string.IsNullOrEmpty(processed)) return dict;
            var parts = processed.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var kv = p.Split(':', 2);
                if (kv.Length == 2 && int.TryParse(kv[0], out var w))
                    dict[w] = $"/uploads/{kv[1]}";
            }
            return dict;
        }
    }

    public class TimelineViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? OriginalImageUrl { get; set; }
        public Dictionary<int, string>? ProcessedImages { get; set; }
        public bool ImageProcessed { get; set; }
    }
}
