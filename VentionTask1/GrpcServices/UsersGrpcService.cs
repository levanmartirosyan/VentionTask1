using Grpc.Core;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.WebApi.Grpc;

namespace VentionTask1.WebApi.GrpcServices
{
    public class UsersGrpcService : UsersGrpc.UsersGrpcBase
    {
        private readonly IUserService _userService;

        public UsersGrpcService(IUserService userService)
        {
            _userService = userService;
        }

        public override async Task<UserGrpcResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.Id, out var userId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user id."));
            }

            var user = await _userService.GetUserByIdAsync(userId, context.CancellationToken);

            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User not found."));
            }

            return new UserGrpcResponse
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                OrganizationId = user.OrganizationId.ToString(),
                OrganizationName = user.OrganizationName ?? string.Empty
            };
        }
    }
}
