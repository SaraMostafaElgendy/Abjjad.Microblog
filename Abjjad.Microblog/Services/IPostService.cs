using Abjjad.Microblog.Models;

namespace Abjjad.Microblog.Services
{
    public interface IPostService
    {
        Task<Post> CreatePostAsync(int authorId, string text, Stream? imageStream, string? fileName, string? contentType);
        Task<IEnumerable<Post>> GetTimelineAsync(int page = 1, int pageSize = 20);
        Task<Post?> GetPostAsync(int id);
    }
}
