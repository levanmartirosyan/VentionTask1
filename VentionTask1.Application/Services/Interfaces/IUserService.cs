using VentionTask1.Application.DTOs;

namespace VentionTask1.Application.Services.Interfaces
{
    public interface IUserService
    {
       Task<List<UserDTO>> GetAllUsersAsync();
       Task<UserDTO?> GetUserByIdAsync(Guid id);
       Task<UserDTO> CreateUserAsync(CreateUserDTO userDTO);
       Task<UserDTO> UpdateUserAsync(Guid id, UpdateUserDTO userDTO);
       Task<bool> DeleteUserAsync(Guid id);
    }
}
