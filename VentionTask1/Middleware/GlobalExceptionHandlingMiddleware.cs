using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VentionTask1.Application.Exceptions;
using VentionTask1.WebApi.Extensions;

namespace VentionTask1.WebApi.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed for request {Path}", context.Request.Path);

                var problem = new ValidationProblemDetails(ex.ToErrorDictionary())
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "One or more validation errors occurred.",
                    Instance = context.Request.Path
                };

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(problem);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized request {Path}", context.Request.Path);

                await WriteProblem(context, StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message);
            }
            catch (ForbiddenAccessException ex)
            {
                _logger.LogWarning(ex, "Forbidden request {Path}", context.Request.Path);

                await WriteProblem(context, StatusCodes.Status403Forbidden, "Forbidden", ex.Message);
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found for request {Path}", context.Request.Path);

                await WriteProblem(context, StatusCodes.Status404NotFound, "Resource not found", ex.Message);
            }
            catch (InvalidOperationException ex) when (
                ex.Message.Contains("already", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(ex, "Conflict for request {Path}", context.Request.Path);

                await WriteProblem(context, StatusCodes.Status409Conflict, "Conflict", ex.Message);
            }
            catch (FileStorageException ex)
            {
                _logger.LogError(ex, "File storage error for request {Path}", context.Request.Path);

                await WriteProblem(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "File storage error",
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error for request {Path}", context.Request.Path);

                await WriteProblem(context, StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred.");
            }
        }

        private static async Task WriteProblem(HttpContext context, int statusCode, string title, string detail)
        {
            var problem = new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{statusCode}",
                Title = title,
                Status = statusCode,
                Detail = detail,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
