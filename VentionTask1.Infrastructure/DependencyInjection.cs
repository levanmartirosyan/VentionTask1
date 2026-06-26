using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Infrastructure.Data;
using VentionTask1.Infrastructure.Repositories.Implementation;

namespace VentionTask1.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    });
            });

            services.AddScoped<IProductRepository, ProductRepository>();

            return services;
        }
    }
}
