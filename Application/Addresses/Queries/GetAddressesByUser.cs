using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Addresses.Queries;

public record GetAddressesByUserQuery(int UserId) : IRequest<IReadOnlyList<AddressResponse>>;

public class GetAddressesByUserHandler : IRequestHandler<GetAddressesByUserQuery, IReadOnlyList<AddressResponse>>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetAddressesByUserHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<AddressResponse>> Handle(GetAddressesByUserQuery request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        if (!await db.Users.AnyAsync(u => u.Id == request.UserId, ct))
        {
            throw new NotFoundException($"User with id {request.UserId} was not found.");
        }

        var addresses = await db.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == request.UserId)
            .OrderBy(a => a.Id)
            .ToListAsync(ct);

        return addresses.Select(AddressResponse.From).ToList();
    }
}
