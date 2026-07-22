using VentionTask1.Application.DTOs;

namespace ApiGateway.Services.Interfaces
{
    public interface ITokenProvider
    {
        (string, DateTime) CreateAccessToken(UserDTO userDTO, int? expiryMinutes = null);
        (string, DateTime) CreateRefreshToken();
    }
}
