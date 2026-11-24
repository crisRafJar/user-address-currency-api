using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Users.Queries;

public record GetUsersQuery(bool? IsActive) : IRequest<IReadOnlyList<UserResponse>>;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserResponse>>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetUsersHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<UserResponse>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var query = db.Users.AsNoTracking();
        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        var users = await query.OrderBy(u => u.Id).ToListAsync(ct);
        return users.Select(UserResponse.From).ToList();
    }
}
