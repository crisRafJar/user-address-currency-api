using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Addresses.Commands;

public record DeleteAddressCommand(int Id) : IRequest;

public class DeleteAddressHandler : IRequestHandler<DeleteAddressCommand>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DeleteAddressHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task Handle(DeleteAddressCommand request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == request.Id, ct)
                      ?? throw new NotFoundException($"Address with id {request.Id} was not found.");

        db.Addresses.Remove(address);
        await db.SaveChangesAsync(ct);
    }
}
