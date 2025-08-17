using Abjjad.Microblog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Abjjad.Microblog.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly IPostService _posts;
        public PostsController(IPostService posts) => _posts = posts;

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [RequestSizeLimit(4 * 1024 * 1024)]
        public async Task<IActionResult> Create(string text, IFormFile? image)
        {
            var userId = int.Parse(User.FindFirst("uid")!.Value);
            Stream? fileStream = null;
            string? filename = null;
            string? contentType = null;

            if (image != null)
            {
                if (!(image.ContentType == "image/jpeg" || image.ContentType == "image/png" || image.ContentType == "image/webp"))
                    return BadRequest("Invalid image type.");
                if (image.Length > 2 * 1024 * 1024) return BadRequest("File too large (max 2MB).");

                fileStream = image.OpenReadStream();
                filename = image.FileName;
                contentType = image.ContentType;
            }

            await _posts.CreatePostAsync(userId, text, fileStream, filename, contentType);
            return RedirectToAction("Index", "Timeline");
        }
    }
}
