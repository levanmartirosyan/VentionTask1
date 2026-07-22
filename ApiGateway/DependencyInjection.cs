using ApiGateway.Services.Implementation;
using ApiGateway.Services.Interfaces;

namespace ApiGateway
{
    public static class DependencyInjection
    {
        public static IHostApplicationBuilder AddApiGateway(
            this IHostApplicationBuilder builder)
        {
            var services = builder.Services;

            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddOpenApi();

            services.AddScoped<ITokenProvider, TokenProvider>();

            services
                .AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            services.AddHttpClient("MainApi", client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration["MainApi:BaseUrl"]
                    ?? throw new InvalidOperationException("MainApi:BaseUrl is missing"));
            });

            return builder;
        }
    }
}
