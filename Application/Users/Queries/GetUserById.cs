using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Users.Queries;

public record GetUserByIdQuery(int Id) : IRequest<UserResponse>;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetUserByIdHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<UserResponse> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.Id, ct)
                   ?? throw new NotFoundException($"User with id {request.Id} was not found.");

        return UserResponse.From(user);
    }
}
