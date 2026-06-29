using VentionTask1.Application.DTOs;

namespace VentionTask1.Application.Services.Interfaces
{
    public interface IUserService
    {
       Task<List<UserDTO>> GetAllUsersAsync(CancellationToken ct);
       Task<UserDTO?> GetUserByIdAsync(Guid id, CancellationToken ct);
       Task<UserDTO> CreateUserAsync(CreateUserDTO userDTO, CancellationToken ct);
       Task<UserDTO> UpdateUserAsync(Guid id, UpdateUserDTO userDTO, CancellationToken ct);
       Task<bool> DeleteUserAsync(Guid id, CancellationToken ct);
    }
}
