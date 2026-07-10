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
        private readonly IOrganizationRepository _organizationRepository;

        public UserService(
            IUserRepository usersRepository,
            IValidator<CreateUserDTO> createUserValidator, 
            IValidator<UpdateUserDTO> updateUserValidator,
            IPasswordService passwordService,
            IOrganizationRepository organizationRepository
            )
        {
            _usersRepository = usersRepository;
            _createUserValidator = createUserValidator;
            _updateUserValidator = updateUserValidator;
            _passwordService = passwordService;
            _organizationRepository = organizationRepository;
        }

        public async Task<PaginatedResponseDTO<UserDTO>> GetUsersPaginatedAsync(Guid? cursor, int pageSize, CancellationToken ct)
        {
            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            var users = await _usersRepository.GetUsersPaginatedAsync(cursor, pageSize, ct);

            var hasNextPage = users.Count > pageSize;

            var items = users
                .Take(pageSize)
                .Select(user => user.ToDto())
                .ToList();

            return new PaginatedResponseDTO<UserDTO>
            {
                Items = items,
                HasNextPage = hasNextPage,
                NextCursor = hasNextPage && items.Any()
                    ? items.Last().Id
                    : null
            };
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid id, CancellationToken ct)
        {
            var user = await _usersRepository.GetUserByIdAsync(id, ct);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' was not found.");
            }

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

            var organization = await _organizationRepository.GetOrganizationByIdAsync(userDTO.OrganizationId, ct);

            if (organization == null)
            {
                throw new KeyNotFoundException($"Organization with ID '{userDTO.OrganizationId}' not found.");
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
                var organization = await _organizationRepository.GetOrganizationByIdAsync(userDTO.OrganizationId.Value, ct);

                if (organization == null)
                {
                    throw new KeyNotFoundException($"Organization with ID '{userDTO.OrganizationId.Value}' not found.");
                }

                user.OrganizationId = userDTO.OrganizationId.Value;
            }

            await _usersRepository.UpdateUserAsync(user);

            if (!await _usersRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }

            return user.ToDto();
        }

        public async Task DeleteUserAsync(Guid id, CancellationToken ct)
        {
            var user = await _usersRepository.GetUserByIdAsync(id, ct);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' was not found.");
            }

            await _usersRepository.DeleteUserAsync(user);

            if (!await _usersRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Internal server error occurred while saving changes.");
            }
        }
    }
}
