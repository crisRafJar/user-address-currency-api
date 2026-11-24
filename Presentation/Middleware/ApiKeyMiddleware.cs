using System.Security.Cryptography;
using System.Text;

namespace UserAddressCurrencyApi.Presentation.Middleware;

public class ApiKeyMiddleware
{
    public const string HeaderName = "X-API-KEY";

    private readonly RequestDelegate _next;
    private readonly byte[] _expectedKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;

        var apiKey = configuration["ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("The 'ApiKey' setting is missing in appsettings.json.");
        }

        _expectedKey = Encoding.UTF8.GetBytes(apiKey);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var provided) ||
            !IsValid(provided.ToString()))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await Results.Problem(
                title: "Unauthorized",
                detail: $"The {HeaderName} header is missing or its value is incorrect.",
                statusCode: StatusCodes.Status401Unauthorized).ExecuteAsync(context);
            return;
        }

        await _next(context);
    }

    private bool IsValid(string provided)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        return CryptographicOperations.FixedTimeEquals(providedBytes, _expectedKey);
    }
}
