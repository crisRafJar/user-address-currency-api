using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Domain.Entities;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.CurrencyConversion;

public record ConvertCurrencyCommand(string FromCurrencyCode, string ToCurrencyCode, decimal Amount)
    : IRequest<ConvertCurrencyResponse>;

public record ConvertCurrencyResponse(
    string FromCurrency,
    string ToCurrency,
    decimal OriginalAmount,
    decimal ConvertedAmount);

public class ConvertCurrencyValidator : AbstractValidator<ConvertCurrencyCommand>
{
    public ConvertCurrencyValidator()
    {
        RuleFor(x => x.FromCurrencyCode).NotEmpty();
        RuleFor(x => x.ToCurrencyCode).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class ConvertCurrencyHandler : IRequestHandler<ConvertCurrencyCommand, ConvertCurrencyResponse>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ConvertCurrencyHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<ConvertCurrencyResponse> Handle(ConvertCurrencyCommand request, CancellationToken ct)
    {
        var fromCode = request.FromCurrencyCode.Trim().ToUpperInvariant();
        var toCode = request.ToCurrencyCode.Trim().ToUpperInvariant();

        var fromTask = FindCurrencyAsync(fromCode, ct);
        var toTask = FindCurrencyAsync(toCode, ct);
        await Task.WhenAll(fromTask, toTask);

        var from = await fromTask ?? throw new NotFoundException($"Currency '{fromCode}' was not found.");
        var to = await toTask ?? throw new NotFoundException($"Currency '{toCode}' was not found.");

        var baseAmount = request.Amount * from.RateToBase;
        var converted = baseAmount / to.RateToBase;

        return new ConvertCurrencyResponse(from.Code, to.Code, request.Amount, converted);
    }

    private async Task<Currency?> FindCurrencyAsync(string code, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code, ct);
    }
}
