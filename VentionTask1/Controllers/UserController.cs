using Microsoft.AspNetCore.Mvc;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.WebApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersPageAsync([FromQuery] Guid? cursor, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            var result = await _userService.GetUsersPaginatedAsync(cursor, pageSize, ct);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIdAsync(Guid id, CancellationToken ct)
        {
            var user = await _userService.GetUserByIdAsync(id, ct);

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDTO userDTO, CancellationToken ct)
        {
            var createdUser = await _userService.CreateUserAsync(userDTO, ct);

            return Ok(createdUser);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(Guid id, [FromBody] UpdateUserDTO userDTO, CancellationToken ct)
        {
            var updatedUser = await _userService.UpdateUserAsync(id, userDTO, ct);

            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(Guid id, CancellationToken ct)
        {
            await _userService.DeleteUserAsync(id, ct);

            return NoContent();
        }
    }
}
