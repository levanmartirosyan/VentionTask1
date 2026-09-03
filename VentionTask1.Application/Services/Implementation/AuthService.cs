using VentionTask1.Application.DTOs;
using VentionTask1.Application.Extensions;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ISessionRepository _sessionRepository;

        public AuthService(IUserRepository userRepository, IPasswordService passwordService, ISessionRepository sessionRepository)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _sessionRepository = sessionRepository;
        }

        public async Task<UserDTO> ValidateLoginAsync(LoginRequestDTO loginRequest, CancellationToken ct)
        {
            var user = await _userRepository.GetUserByEmailAsync(loginRequest.Email, ct);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var isPasswordValid = _passwordService.VerifyPassword(user, user.PasswordHash, loginRequest.Password);

            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return user.ToDto();
        }

        public async Task CreateSessionAsync(CreateSessionDTO createSessionDTO, CancellationToken ct)
        {
            var session = new Session
            {
                UserId = createSessionDTO.UserId,
                LoggedInAt = DateTime.UtcNow,
                IsActive = true,
                IpAddress = createSessionDTO.IpAddress,
                UserAgent = createSessionDTO.UserAgent
            };

            await _sessionRepository.AddAsync(session, ct);

            if (!await _sessionRepository.SaveChangesAsync(ct))
            {
                throw new InvalidOperationException("Session could not be created.");
            }
        }
    }
}
