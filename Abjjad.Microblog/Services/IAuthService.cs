using Abjjad.Microblog.Models;

namespace Abjjad.Microblog.Services
{
    public interface IAuthService
    {
        Task<User?> ValidateCredentialsAsync(string username, string password);
        string GenerateJwt(User user);
    }
}
