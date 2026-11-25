using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Currencies.Commands;

public record UpdateCurrencyRequest(string Code, string Name, decimal RateToBase);

public record UpdateCurrencyCommand(int Id, string Code, string Name, decimal RateToBase) : IRequest<CurrencyResponse>;

public class UpdateCurrencyValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RateToBase).GreaterThan(0);
    }
}

public class UpdateCurrencyHandler : IRequestHandler<UpdateCurrencyCommand, CurrencyResponse>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public UpdateCurrencyHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<CurrencyResponse> Handle(UpdateCurrencyCommand request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var currency = await db.Currencies.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
                       ?? throw new NotFoundException($"Currency with id {request.Id} was not found.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.Currencies.AnyAsync(c => c.Code == code && c.Id != request.Id, ct))
        {
            throw new ConflictException($"Another currency with the code '{code}' already exists.");
        }

        currency.Code = code;
        currency.Name = request.Name.Trim();
        currency.RateToBase = request.RateToBase;
        currency.UpdatedAt = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException($"Another currency with the code '{code}' already exists.");
        }

        return CurrencyResponse.From(currency);
    }
}
