using VentionTask1.Application.DTOs;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.WebApi.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class UserQuery
    {
        public Task<PaginatedResponseDTO<UserDTO>> GetUsers(
            [Service] IUserService userService,
            Guid? cursor,
            int pageSize = 10,
            CancellationToken ct = default)
        {
            return userService.GetUsersPaginatedAsync(
                cursor,
                pageSize,
                ct);
        }
    }
}
