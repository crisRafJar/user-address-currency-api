using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Domain.Entities;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Currencies.Commands;

public record CreateCurrencyCommand(string Code, string Name, decimal RateToBase) : IRequest<CurrencyResponse>;

public class CreateCurrencyValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RateToBase).GreaterThan(0);
    }
}

public class CreateCurrencyHandler : IRequestHandler<CreateCurrencyCommand, CurrencyResponse>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CreateCurrencyHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<CurrencyResponse> Handle(CreateCurrencyCommand request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.Currencies.AnyAsync(c => c.Code == code, ct))
        {
            throw new ConflictException($"A currency with the code '{code}' already exists.");
        }

        var currency = new Currency
        {
            Code = code,
            Name = request.Name.Trim(),
            RateToBase = request.RateToBase
        };

        db.Currencies.Add(currency);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException($"A currency with the code '{code}' already exists.");
        }

        return CurrencyResponse.From(currency);
    }
}
