namespace Abjjad.Microblog.Services
{
    public interface IImageStorage
    {
        Task<string> SaveOriginalAsync(Stream fileStream, string fileName, string contentType);
        Task<string> SaveProcessedAsync(Stream fileStream, string fileName);
        Task<Stream> OpenReadAsync(string relativePath);
        string GetPublicUrl(string relativePath);
    }
}
