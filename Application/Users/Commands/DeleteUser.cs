using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Users.Commands;

public record DeleteUserCommand(int Id) : IRequest;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DeleteUserHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, ct)
                   ?? throw new NotFoundException($"User with id {request.Id} was not found.");

        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
    }
}
