using FluentValidation;
using UserAddressCurrencyApi.Application.Common;

namespace UserAddressCurrencyApi.Presentation.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
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
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            await Results.ValidationProblem(errors).ExecuteAsync(context);
        }
        catch (NotFoundException ex)
        {
            await Results.Problem(title: "Not Found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound)
                .ExecuteAsync(context);
        }
        catch (ConflictException ex)
        {
            await Results.Problem(title: "Conflict", detail: ex.Message, statusCode: StatusCodes.Status409Conflict)
                .ExecuteAsync(context);
        }
        catch (BadHttpRequestException ex)
        {
            await Results.Problem(title: "Bad Request", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest)
                .ExecuteAsync(context);
        }
    }
}
