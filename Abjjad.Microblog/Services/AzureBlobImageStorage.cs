using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Abjjad.Microblog.Services
{
    public class AzureBlobImageStorage : IImageStorage
    {
        private readonly BlobContainerClient _container;
        private readonly string _containerName = "uploads";

        public AzureBlobImageStorage(string connectionString)
        {
            var serviceClient = new BlobServiceClient(connectionString);
            _container = serviceClient.GetBlobContainerClient(_containerName);
            _container.CreateIfNotExists(PublicAccessType.Blob);
        }

        public string GetPublicUrl(string relativePath)
            => $"{_container.Uri}/{relativePath.Replace("\\", "/")}";

        public async Task<string> SaveOriginalAsync(Stream fileStream, string fileName, string contentType)
        {
            var safe = Path.GetRandomFileName() + Path.GetExtension(fileName);
            var blobClient = _container.GetBlobClient(safe);
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });
            return safe;
        }

        public async Task<string> SaveProcessedAsync(Stream fileStream, string fileName)
        {
            var safe = Path.GetRandomFileName() + ".webp";
            var blobClient = _container.GetBlobClient(safe);
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = "image/webp" });
            return safe;
        }

        public async Task<Stream?> OpenReadAsync(string relativePath)
        {
            var blobClient = _container.GetBlobClient(relativePath);
            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.OpenReadAsync();
                return response;
            }
            return null;
        }
    }
}