using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Currencies.Queries;

public record GetCurrencyByIdQuery(int Id) : IRequest<CurrencyResponse>;

public class GetCurrencyByIdHandler : IRequestHandler<GetCurrencyByIdQuery, CurrencyResponse>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetCurrencyByIdHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<CurrencyResponse> Handle(GetCurrencyByIdQuery request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var currency = await db.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.Id, ct)
                       ?? throw new NotFoundException($"Currency with id {request.Id} was not found.");

        return CurrencyResponse.From(currency);
    }
}
