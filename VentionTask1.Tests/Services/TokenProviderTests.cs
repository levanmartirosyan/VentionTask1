using System.Security.Claims;
using ApiGateway.Services.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using VentionTask1.Application.DTOs;
using VentionTask1.Domain.Constants;

namespace VentionTask1.Tests.Services
{
    public class TokenProviderTests
    {
        [Fact]
        public void CreateAccessToken_ShouldReturnTokenWithConfiguredIssuerAudienceAndUserClaims()
        {
            var provider = new TokenProvider(CreateConfiguration());
            var user = CreateUserDto();

            var result = provider.CreateAccessToken(user, 30);
            var jwt = new JsonWebTokenHandler().ReadJsonWebToken(result.Item1);

            Assert.False(string.IsNullOrWhiteSpace(result.Item1));
            Assert.True(result.Item2 > DateTime.UtcNow);
            Assert.Equal("DocuChat.Api", jwt.Issuer);
            Assert.Contains("DocuChat.Client", jwt.Audiences);
            Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == user.Id.ToString());
            Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == user.Email);
            Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Jti);
        }

        [Fact]
        public void CreateRefreshToken_ShouldReturnRandomTokenWithExpiration()
        {
            var provider = new TokenProvider(CreateConfiguration());

            var first = provider.CreateRefreshToken();
            var second = provider.CreateRefreshToken();

            Assert.False(string.IsNullOrWhiteSpace(first.Item1));
            Assert.False(string.IsNullOrWhiteSpace(second.Item1));
            Assert.NotEqual(first.Item1, second.Item1);
            Assert.True(first.Item2 > DateTime.UtcNow.AddDays(14));
        }

        private static IConfiguration CreateConfiguration()
        {
            var values = new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "l@#NV(M*O(N#T*Yynh*UEYHGYUgh736842tgN9B7",
                ["JwtSettings:Issuer"] = "DocuChat.Api",
                ["JwtSettings:Audience"] = "DocuChat.Client",
                ["JwtSettings:ExpiryMinutes"] = "1440"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }

        private static UserDTO CreateUserDto()
        {
            return new UserDTO
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@gmail.com",
                Role = RoleType.MEMBER
            };
        }
    }
}
