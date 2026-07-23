using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ApiGateway.Services.Interfaces;
using VentionTask1.Application.DTOs;

namespace ApiGateway.Services.Implementation
{
    public sealed class TokenProvider : ITokenProvider
    {
        private readonly IConfiguration _configuration;

        public TokenProvider(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public (string, DateTime) CreateAccessToken(UserDTO userDTO, int? expiryMinutes = null)
        {
            string secretKey = _configuration["JwtSettings:SecretKey"]!;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenExpiryMinutes = expiryMinutes ?? _configuration.GetValue<int>("JwtSettings:ExpiryMinutes");
            var expiresAt = DateTime.UtcNow.AddMinutes(tokenExpiryMinutes);

            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userDTO.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, userDTO.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                SigningCredentials = credentials,
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
            };


            var handler = new JsonWebTokenHandler();

            var token = handler.CreateToken(tokenDescriptor);

            return (token, expiresAt);
        }

        public (string, DateTime) CreateRefreshToken()
        {
            var randomBytes = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var refreshTokenString = WebEncoders.Base64UrlEncode(randomBytes);

            var expirationDate = DateTime.UtcNow.AddDays(15);

            return (refreshTokenString, expirationDate);
        }
    }
}
