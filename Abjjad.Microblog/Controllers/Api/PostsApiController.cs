using Abjjad.Microblog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Abjjad.Microblog.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PostsApiController : ControllerBase
    {
        private readonly IPostService _posts;
        private readonly IImageStorage _storage;
        public PostsApiController(IPostService posts, IImageStorage storage) { _posts = posts; _storage = storage; }

        [HttpGet("timeline")]
        public async Task<IActionResult> Timeline([FromQuery]int page = 1)
        {
            var items = await _posts.GetTimelineAsync(page);
            var dto = items.Select(p => new {
                p.Id, p.Text, Username = p.Author?.Username, p.CreatedAt, p.Latitude, p.Longitude,
                OriginalImageUrl = p.OriginalImagePath == null ? null : _storage.GetPublicUrl(p.OriginalImagePath),
                ProcessedImages = ParseProcessedImages(p.ProcessedImages),
                p.ImageProcessed
            });
            return Ok(dto);
        }

        [HttpPost]
        [RequestSizeLimit(4 * 1024 * 1024)]
        public async Task<IActionResult> Create()
        {
            var uidClaim = User.FindFirst("uid")?.Value;
            if (uidClaim == null) return Unauthorized();
            var userId = int.Parse(uidClaim);

            var form = await Request.ReadFormAsync();
            var text = form["text"].FirstOrDefault() ?? string.Empty;
            var file = form.Files.FirstOrDefault();
            Stream? fs = null;
            string? fn = null;
            string? ct = null;
            if (file != null)
            {
                fs = file.OpenReadStream();
                fn = file.FileName;
                ct = file.ContentType;
            }
            var post = await _posts.CreatePostAsync(userId, text, fs, fn, ct);
            return CreatedAtAction(nameof(Get), new { id = post.Id }, new { post.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _posts.GetPostAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
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
}
