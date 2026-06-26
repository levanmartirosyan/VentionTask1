using Microsoft.Extensions.DependencyInjection;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Application.Services.Interfaces;

namespace VentionTask1.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();

            return services;
        }
    }
}
