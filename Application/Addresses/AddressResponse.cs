using UserAddressCurrencyApi.Domain.Entities;

namespace UserAddressCurrencyApi.Application.Addresses;

public record AddressResponse(int Id, int UserId, string Street, string City, string Country, string? ZipCode, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static AddressResponse From(Address a) =>
        new(a.Id, a.UserId, a.Street, a.City, a.Country, a.ZipCode, a.CreatedAt, a.UpdatedAt);
}
