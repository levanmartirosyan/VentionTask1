using FluentValidation;

namespace VentionTask1.WebApi.Extensions
{
    public static class ValidationExceptionExtensions
    {
        public static Dictionary<string, string[]> ToErrorDictionary(this ValidationException exception)
        {
            return exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());
        }
    }
}
