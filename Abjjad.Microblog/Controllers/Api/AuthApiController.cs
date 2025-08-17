using Abjjad.Microblog.Services;
using Microsoft.AspNetCore.Mvc;

namespace Abjjad.Microblog.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthApiController(IAuthService auth) => _auth = auth;

        [HttpPost("token")]
        public async Task<IActionResult> Token([FromBody] LoginRequest req)
        {
            var user = await _auth.ValidateCredentialsAsync(req.Username, req.Password);
            if (user == null) return Unauthorized(new { error = "invalid_grant" });
            var jwt = _auth.GenerateJwt(user);
            return Ok(new { accessToken = jwt, tokenType = "bearer", expiresIn = 21600 });
        }

        public record LoginRequest(string Username, string Password);
    }
}
