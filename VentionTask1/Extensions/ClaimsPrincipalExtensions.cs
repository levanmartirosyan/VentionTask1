using System.Security.Claims;
using VentionTask1.Domain.Constants;

namespace VentionTask1.WebApi.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(value, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user id claim.");
            }

            return userId;
        }

        public static RoleType GetPlatformRole(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.Role);

            if (!Enum.TryParse<RoleType>(value, out var role))
            {
                throw new UnauthorizedAccessException("Invalid user role claim.");
            }

            return role;
        }
    }
}
