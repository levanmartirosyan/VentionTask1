using FluentValidation;
using MassTransit;
using System.Text.Json;
using System.Text.Json.Serialization;
using VentionTask1.Application.Consumers;
using VentionTask1.Settings;
using VentionTask1.WebApi.Settings;

namespace VentionTask1
{
    public static class DependencyInjection
    {
        public static IHostApplicationBuilder AddWebApi(
            this IHostApplicationBuilder builder)
        {
            var services = builder.Services;

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            services.AddMassTransit(x =>
            {
                x.AddConsumer<FileProcessingRequestedConsumer>();
                x.AddConsumer<FileTextExtractedConsumer>();
                x.AddConsumer<FileChunkingCompletedConsumer>();
                x.AddConsumer<FileProcessingCompletedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqOptions = builder.Configuration
                        .GetSection("RabbitMQ")
                        .Get<RabbitMqOptions>() ?? new RabbitMqOptions();

                    cfg.Host(rabbitMqOptions.Host, rabbitMqOptions.Port, "/", h =>
                    {
                        h.Username(rabbitMqOptions.Username);
                        h.Password(rabbitMqOptions.Password);
                    });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Ignore<ValidationException>();
                        r.Ignore<FileNotFoundException>();

                        r.Exponential(
                            retryLimit: 3,
                            minInterval: TimeSpan.FromSeconds(2),
                            maxInterval: TimeSpan.FromSeconds(30),
                            intervalDelta: TimeSpan.FromSeconds(5));
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddGrpc();

            services.AddOpenApi();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.Configure<ApplicationSettings>(
                builder.Configuration.GetSection("ApplicationSettings"));
            services.Configure<RabbitMqOptions>(
                builder.Configuration.GetSection("RabbitMQ"));

            return builder;
        }
    }
}
