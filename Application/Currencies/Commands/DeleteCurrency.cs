using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Currencies.Commands;

public record DeleteCurrencyCommand(int Id) : IRequest;

public class DeleteCurrencyHandler : IRequestHandler<DeleteCurrencyCommand>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DeleteCurrencyHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task Handle(DeleteCurrencyCommand request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var currency = await db.Currencies.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
                       ?? throw new NotFoundException($"Currency with id {request.Id} was not found.");

        db.Currencies.Remove(currency);
        await db.SaveChangesAsync(ct);
    }
}
