using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VentionTask1.Application.Consumers;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Settings;
using VentionTask1.WebApi.GraphQL;
using VentionTask1.WebApi.GraphQL.Mutations;
using VentionTask1.WebApi.GraphQL.Queries;
using VentionTask1.WebApi.GraphQL.Types;
using VentionTask1.WebApi.Services.Implementation;
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

            services.AddHttpContextAccessor();

            services.AddMassTransit(x =>
            {
                var rabbitMqOptions = builder.Configuration
                    .GetSection("RabbitMQ")
                    .Get<RabbitMqOptions>() ?? new RabbitMqOptions();

                x.AddConsumer<FileProcessingRequestedConsumer>();
                x.AddConsumer<FileTextExtractedConsumer>();
                x.AddConsumer<FileChunkingCompletedConsumer>();
                x.AddConsumer<FileProcessingCompletedConsumer>();

                x.AddConfigureEndpointsCallback((_, endpointConfigurator) =>
                {
                    endpointConfigurator.ConcurrentMessageLimit =
                        rabbitMqOptions.ConcurrentMessageLimit;

                    if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbitMqEndpoint)
                    {
                        rabbitMqEndpoint.PrefetchCount =
                            rabbitMqOptions.PrefetchCount;

                        rabbitMqEndpoint.SetQueueArgument(
                            "x-dead-letter-exchange",
                            rabbitMqOptions.DeadLetterExchange);

                        rabbitMqEndpoint.SetQueueArgument(
                            "x-dead-letter-routing-key",
                            rabbitMqOptions.DeadLetterRoutingKey);
                    }
                });

                x.UsingRabbitMq((context, cfg) =>
                {

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

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,

                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                           Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrWhiteSpace(accessToken) &&
                                path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();

            services.AddGrpc();

            services.AddSignalR(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.MaximumReceiveMessageSize = 32 * 1024;
            });

            services
                .AddGraphQLServer()
                .AddQueryType(d => d.Name("Query"))
                .AddMutationType(d => d.Name("Mutation"))
                .AddTypeExtension<OrganizationQuery>()
                .AddTypeExtension<UserQuery>()
                .AddTypeExtension<OrganizationMutation>()
                .AddTypeExtension<UserType>();

            services.AddOpenApi();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        []
                    }
                });
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;
            });

            services.Configure<ApplicationSettings>(
                builder.Configuration.GetSection("ApplicationSettings"));
            services.Configure<RabbitMqOptions>(
                builder.Configuration.GetSection("RabbitMQ"));

            services.AddScoped<IFileProcessingNotifier, FileProcessingSignalRNotifier>();
            services.AddSingleton<IUserPresenceTracker, InMemoryUserPresenceTracker>();

            return builder;
        }
    }
}
