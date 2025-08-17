using Xunit;
using Abjjad.Microblog.Services;
using Abjjad.Microblog.Data;
using Abjjad.Microblog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;


namespace Abjjad.Microblog.Tests
{
    public class AuthServiceTests
    {
       private AuthService CreateAuthService()
{
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB for every test
        .Options;

    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "super_secret_test_key_1234567890123456", // ✅ 32+ chars
    ["Jwt:Issuer"] = "test_issuer",
    ["Jwt:Audience"] = "test_audience"
        })
        .Build();

    var db = new AppDbContext(options);
    db.Users.Add(new User { Id = 1, Username = "admin", PasswordHash = "password" });
    db.SaveChanges();

    return new AuthService(db, config);
}

        [Fact]
        public async Task ValidateCredentialsAsync_ShouldFail_ForInvalidCredentials()
        {
            var auth = CreateAuthService();
            var result = await auth.ValidateCredentialsAsync("wrong", "wrong");
            Assert.Null(result); // invalid => no user
        }

        [Fact]
        public async Task ValidateCredentialsAsync_ShouldPass_ForValidCredentials()
        {
            var auth = CreateAuthService();
            var result = await auth.ValidateCredentialsAsync("admin", "password");
            Assert.NotNull(result); // valid => user exists
        }

        [Fact]
        public async Task GenerateJwt_ShouldReturnToken_ForValidUser()
        {
            var auth = CreateAuthService();
            var user = await auth.ValidateCredentialsAsync("admin", "password");
            Assert.NotNull(user);

            var token = auth.GenerateJwt(user!);
            Assert.False(string.IsNullOrEmpty(token)); // token should not be empty
        }
    }
}
