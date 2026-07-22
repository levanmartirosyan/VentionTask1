using VentionTask1.Application.DTOs;

namespace VentionTask1.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDTO> ValidateLoginAsync(LoginRequestDTO loginRequest, CancellationToken ct);
    }
}
