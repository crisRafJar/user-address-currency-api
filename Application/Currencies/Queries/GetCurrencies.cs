using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Currencies.Queries;

public record GetCurrenciesQuery : IRequest<IReadOnlyList<CurrencyResponse>>;

public class GetCurrenciesHandler : IRequestHandler<GetCurrenciesQuery, IReadOnlyList<CurrencyResponse>>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GetCurrenciesHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<CurrencyResponse>> Handle(GetCurrenciesQuery request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var currencies = await db.Currencies.AsNoTracking().OrderBy(c => c.Code).ToListAsync(ct);
        return currencies.Select(CurrencyResponse.From).ToList();
    }
}
