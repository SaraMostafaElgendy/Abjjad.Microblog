using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Abjjad.Microblog.Data;
using Abjjad.Microblog.Models;
using Abjjad.Microblog.Services;
using Abjjad.Microblog.Background;
using System.Collections.Generic;


namespace Abjjad.Microblog.Tests
{
    public class PostServiceTests
    {
        private AppDbContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
                .Options;

            return new AppDbContext(options);
        }

        private class FakeImageStorage : IImageStorage
        {
            private readonly Dictionary<string, byte[]> _store = new();

            public Task<string> SaveOriginalAsync(Stream stream, string fileName, string contentType)
            {
                return Task.FromResult($"saved/{fileName}");
            }

            public Task<string> SaveProcessedAsync(Stream stream, string fileName)
            {
                return Task.FromResult($"processed/{fileName}");
            }

            public string GetPublicUrl(string path)
            {
                return $"http://fake.test/{path}";
            }
            public Task<Stream> OpenReadAsync(string relativePath)
            {
                if (_store.TryGetValue(Path.GetFileName(relativePath), out var data))
                {
                    return Task.FromResult<Stream>(new MemoryStream(data));
                }

                throw new FileNotFoundException("Test file not found", relativePath);
            }
        }

        [Fact]
        public async Task CreatePostAsync_ShouldSavePostWithoutImage()
        {
            using var db = GetInMemoryDb();
            var storage = new FakeImageStorage();
            var queue = new ImageProcessingQueue();
            var service = new PostService(db, storage, queue);

            var post = await service.CreatePostAsync(1, "Hello World", null, null, null);

            Assert.NotNull(post);
            Assert.Equal("Hello World", post.Text);
            Assert.True(db.Posts.Any());
            Assert.Null(post.OriginalImagePath);
        }

        [Fact]
        public async Task CreatePostAsync_ShouldSavePostWithImage()
        {
            using var db = GetInMemoryDb();
            var storage = new FakeImageStorage();
            var queue = new ImageProcessingQueue();
            var service = new PostService(db, storage, queue);

            using var ms = new MemoryStream(new byte[] { 1, 2, 3 });
            var post = await service.CreatePostAsync(1, "Post with image", ms, "pic.jpg", "image/jpeg");

            Assert.NotNull(post.OriginalImagePath);
            Assert.False(post.ImageProcessed);
            Assert.Contains("saved/pic.jpg", post.OriginalImagePath);
        }

        [Fact]

        public async Task GetPostAsync_ShouldReturnPost()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var db = new AppDbContext(options);
            var storage = new FakeImageStorage();
            var queue = new ImageProcessingQueue();
            var service = new PostService(db, storage, queue);

            var user = new User { Id = 1, Username = "author", PasswordHash = "dummyhash" };
            db.Users.Add(user);
            db.SaveChanges();

            var post = new Post { Id = 1, AuthorId = 1, Text = "test post", CreatedAt = DateTime.UtcNow };
            db.Posts.Add(post);
            db.SaveChanges();

            var result = await service.GetPostAsync(1);
            Assert.NotNull(result);
            Assert.Equal("test post", result!.Text);
            Assert.Equal("author", result.Author.Username);
        }

        [Fact]
        public async Task GetTimelineAsync_ShouldReturnPostsOrdered()
        {
            using var db = GetInMemoryDb();
            var storage = new FakeImageStorage();
            var queue = new ImageProcessingQueue();
            var service = new PostService(db, storage, queue);

            // ✅ Create valid users first
            var user1 = new User
            {
                Id = 1,
                Username = "Alice",
                PasswordHash = "hash1"
            };
            var user2 = new User
            {
                Id = 2,
                Username = "Bob",
                PasswordHash = "hash2"
            };

            db.Users.AddRange(user1, user2);
            db.SaveChanges();

            // ✅ Now create posts for those users
            await service.CreatePostAsync(1, "First", null, null, null);
            await Task.Delay(10); // ensure CreatedAt difference
            await service.CreatePostAsync(2, "Second", null, null, null);

            var timeline = await service.GetTimelineAsync();

            // Assert
            Assert.Equal(2, timeline.Count());
            Assert.Equal("Second", timeline.First().Text); // most recent should be first
        }
    }
}


