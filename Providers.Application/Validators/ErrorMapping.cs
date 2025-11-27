using FluentResults;

namespace Providers.Application.Validators
{
    public static class ErrorMapping
    {
        public static Dictionary<string, string> ToDictionary(this IReadOnlyList<IError> errors)
        {
            return errors
                .GroupBy(e =>
                {
                    if (e.Metadata.TryGetValue("field", out var f) && f is not null)
                        return f.ToString() ?? string.Empty;

                    return string.Empty; // errores sin campo → clave vacía
                })
                .ToDictionary(
                    g => g.Key,
                    g => string.Join("; ", g.Select(e => e.Message))
                );
        }
    }
}
