using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Application.Validators.User;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IPasswordService, PasswordService>();

            services.AddValidatorsFromAssemblyContaining<CreateUserDTOValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateUserDTOValidator>();

            return services;
        }
    }
}
