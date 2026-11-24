using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Addresses.Commands;

public record UpdateAddressCommand(int Id, string Street, string City, string Country, string? ZipCode)
    : IRequest<AddressResponse>;

public class UpdateAddressValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(300);
        RuleFor(x => x.City).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ZipCode).MaximumLength(20);
    }
}

public class UpdateAddressHandler : IRequestHandler<UpdateAddressCommand, AddressResponse>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public UpdateAddressHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<AddressResponse> Handle(UpdateAddressCommand request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == request.Id, ct)
                      ?? throw new NotFoundException($"Address with id {request.Id} was not found.");

        address.Street = request.Street.Trim();
        address.City = request.City.Trim();
        address.Country = request.Country.Trim();
        address.ZipCode = string.IsNullOrWhiteSpace(request.ZipCode) ? null : request.ZipCode.Trim();
        address.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return AddressResponse.From(address);
    }
}
