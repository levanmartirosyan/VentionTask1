using VentionTask1.Settings;

namespace VentionTask1
{
    public static class DependencyInjection
    {
        public static IHostApplicationBuilder AddWebApi(
            this IHostApplicationBuilder builder)
        {
            var services = builder.Services;

            services.AddControllers();
            services.AddOpenApi();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.Configure<ApplicationSettings>(
                builder.Configuration.GetSection("ApplicationSettings"));

            return builder;
        }
    }
}
