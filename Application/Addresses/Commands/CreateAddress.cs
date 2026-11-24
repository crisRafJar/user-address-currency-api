using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Domain.Entities;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Addresses.Commands;

public record AddressRequest(string Street, string City, string Country, string? ZipCode = null);

public record CreateAddressCommand(int UserId, string Street, string City, string Country, string? ZipCode)
    : IRequest<AddressResponse>;

public class CreateAddressValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(300);
        RuleFor(x => x.City).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ZipCode).MaximumLength(20);
    }
}

public class CreateAddressHandler : IRequestHandler<CreateAddressCommand, AddressResponse>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CreateAddressHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<AddressResponse> Handle(CreateAddressCommand request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        if (!await db.Users.AnyAsync(u => u.Id == request.UserId, ct))
        {
            throw new NotFoundException($"User with id {request.UserId} was not found.");
        }

        var address = new Address
        {
            UserId = request.UserId,
            Street = request.Street.Trim(),
            City = request.City.Trim(),
            Country = request.Country.Trim(),
            ZipCode = string.IsNullOrWhiteSpace(request.ZipCode) ? null : request.ZipCode.Trim()
        };

        db.Addresses.Add(address);
        await db.SaveChangesAsync(ct);

        return AddressResponse.From(address);
    }
}
