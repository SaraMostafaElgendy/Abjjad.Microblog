using Microsoft.AspNetCore.Hosting;

namespace Abjjad.Microblog.Services
{
    public class LocalImageStorage : IImageStorage
    {
        private readonly string _root;
        private readonly IWebHostEnvironment _env;
        public LocalImageStorage(IWebHostEnvironment env)
        {
            _env = env;
            _root = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(_root);
        }

        public string GetPublicUrl(string relativePath) => $"/uploads/{relativePath.Replace('\\','/')}";
        public async Task<string> SaveOriginalAsync(Stream fileStream, string fileName, string contentType)
        {
            var safe = Path.GetRandomFileName() + Path.GetExtension(fileName);
            var full = Path.Combine(_root, safe);
            using var fs = File.Create(full);
            await fileStream.CopyToAsync(fs);
            return safe;
        }
        public async Task<string> SaveProcessedAsync(Stream fileStream, string fileName)
        {
            var safe = Path.GetRandomFileName() + ".webp";
            var full = Path.Combine(_root, safe);
            using var fs = File.Create(full);
            fileStream.Seek(0, SeekOrigin.Begin);
            await fileStream.CopyToAsync(fs);
            return safe;
        }
        public Task<Stream?> OpenReadAsync(string relativePath)
        {
            var full = Path.Combine(_root, relativePath);
            if (!File.Exists(full)) return Task.FromResult<Stream?>(null);
            return Task.FromResult<Stream?>(File.OpenRead(full));
        }
    }
}
