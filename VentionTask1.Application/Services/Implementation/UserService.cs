using FluentValidation;
using VentionTask1.Application.DTOs;
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

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = await _usersRepository.GetAllUsersAsync();

            return users.Select(user => new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                OrganizationId = user.OrganizationId,
                OrganizationName = user.Organization.Name
            }).ToList();
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid id)
        {
            var user = await _usersRepository.GetUserByIdAsync(id);

            if (user == null) return null;

            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                OrganizationId = user.OrganizationId,
                OrganizationName = user.Organization.Name
            };
        }

        public async Task<UserDTO> CreateUserAsync(CreateUserDTO userDTO)
        {
            var validationResult = await _createUserValidator.ValidateAsync(userDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingUser = await _usersRepository.GetUserByEmailAsync(userDTO.Email);

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

            var createdUser = await _usersRepository.CreateUserAsync(newUser);

            if (!await _usersRepository.SaveChangesAsync())
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            var savedUser = await _usersRepository.GetUserByIdAsync(createdUser.Id);

            if (savedUser == null)
            {
                throw new InvalidOperationException("Created user was not found.");
            }

            return new UserDTO
            {
                Id = savedUser.Id,
                Username = savedUser.Username,
                Email = savedUser.Email,
                OrganizationId = savedUser.OrganizationId,
                OrganizationName = savedUser.Organization.Name
            };
        }

        public async Task<UserDTO> UpdateUserAsync(Guid id, UpdateUserDTO userDTO)
        {
            var validationResult = await _updateUserValidator.ValidateAsync(userDTO);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var user = await _usersRepository.GetUserByIdAsync(id);

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

            if (!await _usersRepository.SaveChangesAsync())
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            var updatedUser = await _usersRepository.GetUserByIdAsync(id);

            if (updatedUser == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            return new UserDTO
            {
                Id = updatedUser.Id,
                Username = updatedUser.Username,
                Email = updatedUser.Email,
                OrganizationId = updatedUser.OrganizationId,
                OrganizationName = updatedUser.Organization.Name
            };
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _usersRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return false;
            }

            await _usersRepository.DeleteUserAsync(user);

            if (!await _usersRepository.SaveChangesAsync())
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            return true;
        }
    }
}
