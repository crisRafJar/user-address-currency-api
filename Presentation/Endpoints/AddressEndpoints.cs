using MediatR;
using UserAddressCurrencyApi.Application.Addresses;
using UserAddressCurrencyApi.Application.Addresses.Commands;
using UserAddressCurrencyApi.Application.Addresses.Queries;

namespace UserAddressCurrencyApi.Presentation.Endpoints;

public static class AddressEndpoints
{
    public static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/users/{userId:int}/addresses",
                async (int userId, AddressRequest body, IMediator mediator, CancellationToken ct) =>
                {
                    var address = await mediator.Send(
                        new CreateAddressCommand(userId, body.Street, body.City, body.Country, body.ZipCode), ct);
                    return Results.Created($"/addresses/{address.Id}", address);
                })
            .WithTags("Addresses")
            .WithName("CreateAddress")
            .Produces<AddressResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        routes.MapGet("/users/{userId:int}/addresses", async (int userId, IMediator mediator, CancellationToken ct) =>
                Results.Ok(await mediator.Send(new GetAddressesByUserQuery(userId), ct)))
            .WithTags("Addresses")
            .WithName("GetAddressesByUser")
            .Produces<IReadOnlyList<AddressResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        var group = routes.MapGroup("/addresses").WithTags("Addresses");

        group.MapPut("/{id:int}", async (int id, AddressRequest body, IMediator mediator, CancellationToken ct) =>
                Results.Ok(await mediator.Send(
                    new UpdateAddressCommand(id, body.Street, body.City, body.Country, body.ZipCode), ct)))
            .WithName("UpdateAddress")
            .Produces<AddressResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteAddressCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteAddress")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return routes;
    }
}
