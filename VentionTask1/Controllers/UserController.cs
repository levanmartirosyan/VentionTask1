using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.WebApi.Extensions;

namespace VentionTask1.WebApi.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOrganizationPermissionService _permissionService;

        public UserController(IUserService userService, IOrganizationPermissionService permissionService)
        {
            _userService = userService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersPageAsync([FromQuery] Guid? cursor, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            var role = User.GetPlatformRole();
            _permissionService.EnsurePlatformAdminOrOwner(role);

            var result = await _userService.GetUsersPaginatedAsync(cursor, pageSize, ct);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIdAsync(Guid id, CancellationToken ct)
        {
            var currentUserId = User.GetUserId();
            var role = User.GetPlatformRole();

            _permissionService.EnsureCanReadUser(currentUserId, role, id);

            var user = await _userService.GetUserByIdAsync(id, ct);

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDTO userDTO, CancellationToken ct)
        {
            var role = User.GetPlatformRole();
            _permissionService.EnsurePlatformAdminOrOwner(role);

            var createdUser = await _userService.CreateUserAsync(userDTO, ct);

            return Ok(createdUser);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(Guid id, [FromBody] UpdateUserDTO userDTO, CancellationToken ct)
        {
            var currentUserId = User.GetUserId();
            var role = User.GetPlatformRole();

            _permissionService.EnsureCanReadUser(currentUserId, role, id);

            var updatedUser = await _userService.UpdateUserAsync(id, userDTO, ct);

            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(Guid id, CancellationToken ct)
        {
            var role = User.GetPlatformRole();
            _permissionService.EnsurePlatformAdminOrOwner(role);

            await _userService.DeleteUserAsync(id, ct);

            return NoContent();
        }
    }
}
