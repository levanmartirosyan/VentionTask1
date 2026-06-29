using FluentValidation;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Extensions;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _usersRepository;
        private readonly IValidator<CreateUserDTO> _createUserValidator;
        private readonly IValidator<UpdateUserDTO> _updateUserValidator;
        private readonly IPasswordService _passwordService;

        public UserService(
            IUserRepository usersRepository,
            IValidator<CreateUserDTO> createUserValidator, 
            IValidator<UpdateUserDTO> updateUserValidator,
            IPasswordService passwordService)
        {
            _usersRepository = usersRepository;
            _createUserValidator = createUserValidator;
            _updateUserValidator = updateUserValidator;
            _passwordService = passwordService;
        }

        public async Task<List<UserDTO>> GetAllUsersAsync(CancellationToken ct)
        {
            var users = await _usersRepository.GetAllUsersAsync(ct);

            return users.Select(user => user.ToDto()).ToList();
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid id, CancellationToken ct)
        {
            var user = await _usersRepository.GetUserByIdAsync(id, ct);

            if (user == null) return null;

            return user.ToDto();
        }

        public async Task<UserDTO> CreateUserAsync(CreateUserDTO userDTO, CancellationToken ct)
        {
            var validationResult = await _createUserValidator.ValidateAsync(userDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingUser = await _usersRepository.GetUserByEmailAsync(userDTO.Email, ct);

            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with the same email already exists.");
            }

            var newUser = new User
            {
                Username = userDTO.Username,
                Email = userDTO.Email,
                PasswordHash = string.Empty,
                OrganizationId = userDTO.OrganizationId
            };

            newUser.PasswordHash = _passwordService.HashPassword(newUser, userDTO.Password);

            var createdUser = await _usersRepository.CreateUserAsync(newUser, ct);

            if (!await _usersRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            return createdUser.ToDto();
        }

        public async Task<UserDTO> UpdateUserAsync(Guid id, UpdateUserDTO userDTO, CancellationToken ct)
        {
            var validationResult = await _updateUserValidator.ValidateAsync(userDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var user = await _usersRepository.GetUserByIdAsync(id, ct);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (!string.IsNullOrWhiteSpace(userDTO.Username))
            {
                user.Username = userDTO.Username;
            }

            if (!string.IsNullOrWhiteSpace(userDTO.Email))
            {
                var existingUser = await _usersRepository.GetUserByEmailAsync(userDTO.Email, ct);

                if (existingUser != null && existingUser.Id != user.Id)
                {
                    throw new InvalidOperationException("A user with the same email already exists.");
                }

                user.Email = userDTO.Email;
            }

            if (!string.IsNullOrWhiteSpace(userDTO.NewPassword))
            {
                user.PasswordHash = _passwordService.HashPassword(user, userDTO.NewPassword);
            }

            if (userDTO.OrganizationId.HasValue)
            {
                user.OrganizationId = userDTO.OrganizationId.Value;
            }

            await _usersRepository.UpdateUserAsync(user);

            if (!await _usersRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            return user.ToDto();
        }

        public async Task<bool> DeleteUserAsync(Guid id, CancellationToken ct)
        {
            var user = await _usersRepository.GetUserByIdAsync(id, ct);

            if (user == null)
            {
                return false;
            }

            await _usersRepository.DeleteUserAsync(user);

            if (!await _usersRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            return true;
        }
    }
}
