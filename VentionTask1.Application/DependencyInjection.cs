using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Application.Validators.User;

namespace VentionTask1.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPasswordService, PasswordService>();

            services.AddValidatorsFromAssemblyContaining<CreateUserDTOValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateUserDTOValidator>();

            return services;
        }
    }
}
