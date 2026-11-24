using UserAddressCurrencyApi.Domain.Entities;

namespace UserAddressCurrencyApi.Application.Currencies;

public record CurrencyResponse(int Id, string Code, string Name, decimal RateToBase, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static CurrencyResponse From(Currency c) =>
        new(c.Id, c.Code, c.Name, c.RateToBase, c.CreatedAt, c.UpdatedAt);
}
