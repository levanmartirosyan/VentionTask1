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

            var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/validate-login");

            request.Content = JsonContent.Create(loginRequest);

            if (HttpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            {
                request.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId.ToString());
            }

            var response = await client.SendAsync(request, ct);

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

            var createSessionResponse = await client.PostAsJsonAsync(
                "api/auth/sessions",
                new CreateSessionDTO
                {
                    UserId = user.Id,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers.UserAgent.ToString()
                },
                ct);

            if (!createSessionResponse.IsSuccessStatusCode)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

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
