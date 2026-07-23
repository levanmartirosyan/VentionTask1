using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VentionTask1.Application.Exceptions;
using VentionTask1.WebApi.Extensions;

namespace VentionTask1.WebApi.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
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
                await WriteProblem(context, StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                await WriteProblem(context, StatusCodes.Status404NotFound, "Resource not found", ex.Message);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                await WriteProblem(context, StatusCodes.Status409Conflict, "Conflict", ex.Message);
            }
            catch (FileStorageException ex)
            {
                await WriteProblem(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "File storage error",
                    ex.Message);
            }
            catch (Exception)
            {
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
