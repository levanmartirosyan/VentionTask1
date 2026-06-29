using VentionTask1.Application.DTOs;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Extensions
{
    public static class UserMappingExtensions
    {
        public static UserDTO ToDto(this User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                OrganizationId = user.OrganizationId,
                OrganizationName = user.Organization?.Name
            };
        }
    }
}
