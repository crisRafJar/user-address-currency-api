using MediatR;
using UserAddressCurrencyApi.Application.Currencies;
using UserAddressCurrencyApi.Application.Currencies.Commands;
using UserAddressCurrencyApi.Application.Currencies.Queries;
using UserAddressCurrencyApi.Application.CurrencyConversion;

namespace UserAddressCurrencyApi.Presentation.Endpoints;

public static class CurrencyEndpoints
{
    public static IEndpointRouteBuilder MapCurrencyEndpoints(this IEndpointRouteBuilder routes)
    {
        var currencies = routes.MapGroup("/currencies").WithTags("Currencies");

        currencies.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
                Results.Ok(await mediator.Send(new GetCurrenciesQuery(), ct)))
            .WithName("GetCurrencies")
            .Produces<IReadOnlyList<CurrencyResponse>>();

        currencies.MapGet("/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
                Results.Ok(await mediator.Send(new GetCurrencyByIdQuery(id), ct)))
            .WithName("GetCurrencyById")
            .Produces<CurrencyResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        currencies.MapPost("/", async (CreateCurrencyCommand body, IMediator mediator, CancellationToken ct) =>
            {
                var currency = await mediator.Send(body, ct);
                return Results.Created($"/currencies/{currency.Id}", currency);
            })
            .WithName("CreateCurrency")
            .Produces<CurrencyResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        currencies.MapPut("/{id:int}", async (int id, UpdateCurrencyRequest body, IMediator mediator, CancellationToken ct) =>
                Results.Ok(await mediator.Send(new UpdateCurrencyCommand(id, body.Code, body.Name, body.RateToBase), ct)))
            .WithName("UpdateCurrency")
            .Produces<CurrencyResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        currencies.MapDelete("/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteCurrencyCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteCurrency")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routes.MapPost("/currency/convert", async (ConvertCurrencyCommand body, IMediator mediator, CancellationToken ct) =>
                Results.Ok(await mediator.Send(body, ct)))
            .WithTags("Currencies")
            .WithName("ConvertCurrency")
            .Produces<ConvertCurrencyResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return routes;
    }
}
