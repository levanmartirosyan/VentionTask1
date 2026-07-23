using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.WebApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("validate-login")]
        public async Task<ActionResult<UserDTO>> ValidateLogin(LoginRequestDTO loginRequest, CancellationToken ct)
        {
            var user = await _authService.ValidateLoginAsync(loginRequest, ct);

            return Ok(user);
        }
    }
}
