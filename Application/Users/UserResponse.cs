using UserAddressCurrencyApi.Domain.Entities;

namespace UserAddressCurrencyApi.Application.Users;

public record UserResponse(int Id, string Name, string Email, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static UserResponse From(User u) => new(u.Id, u.Name, u.Email, u.IsActive, u.CreatedAt, u.UpdatedAt);
}
