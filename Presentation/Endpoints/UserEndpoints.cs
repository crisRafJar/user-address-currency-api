using MediatR;
using UserAddressCurrencyApi.Application.Users;
using UserAddressCurrencyApi.Application.Users.Commands;
using UserAddressCurrencyApi.Application.Users.Queries;

namespace UserAddressCurrencyApi.Presentation.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/users").WithTags("Users");

        group.MapPost("/", async (CreateUserRequest body, IMediator mediator, CancellationToken ct) =>
            {
                var user = await mediator.Send(new CreateUserCommand(body.Name, body.Email, body.Password), ct);
                return Results.Created($"/users/{user.Id}", user);
            })
            .WithName("CreateUser")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/", async (bool? isActive, IMediator mediator, CancellationToken ct) =>
                Results.Ok(await mediator.Send(new GetUsersQuery(isActive), ct)))
            .WithName("GetUsers")
            .Produces<IReadOnlyList<UserResponse>>();

        group.MapGet("/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
                Results.Ok(await mediator.Send(new GetUserByIdQuery(id), ct)))
            .WithName("GetUserById")
            .Produces<UserResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:int}", async (int id, UpdateUserRequest body, IMediator mediator, CancellationToken ct) =>
                Results.Ok(await mediator.Send(new UpdateUserCommand(id, body.Name, body.Email, body.IsActive), ct)))
            .WithName("UpdateUser")
            .Produces<UserResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteUserCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/bulk", async (BulkCreateUsersCommand body, IMediator mediator, CancellationToken ct) =>
                Results.Ok(await mediator.Send(body, ct)))
            .WithName("BulkCreateUsers")
            .Produces<BulkCreateUsersResult>()
            .ProducesValidationProblem();

        return routes;
    }
}
