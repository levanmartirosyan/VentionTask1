using VentionTask1.Domain.Entities;

namespace VentionTask1.Application.Services.Interfaces
{
    public interface IPasswordService
    {
        public string HashPassword(User user, string password);
        public bool VerifyPassword(User user, string hashedPassword, string providedPassword);
    }
}
