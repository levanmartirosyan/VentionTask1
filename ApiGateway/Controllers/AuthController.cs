using ApiGateway.DTOs;
using ApiGateway.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using VentionTask1.Application.DTOs;

namespace ApiGateway.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenProvider _tokenProvider;

        public AuthController(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequestDTO loginRequest, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient("MainApi");

            var response = await client.PostAsJsonAsync("api/auth/validate-login", loginRequest, ct);

            if (!response.IsSuccessStatusCode)
            {
                return Unauthorized();
            }

            var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            jsonOptions.Converters.Add(new JsonStringEnumConverter());

            var user = await response.Content.ReadFromJsonAsync<UserDTO>(jsonOptions, cancellationToken: ct);

            if (user is null)
            {
                return Unauthorized();
            }

            var (accessToken, _) = _tokenProvider.CreateAccessToken(user);

            return Ok(new LoginResponseDTO
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                Image = null,
                AccessToken = accessToken
            });
        }
    }
}
