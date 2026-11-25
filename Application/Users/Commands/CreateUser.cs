using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Domain.Entities;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Users.Commands;

public record CreateUserRequest(string Name, string Email, string? Password = null);

public record CreateUserCommand(string Name, string Email, string? Password = null) : IRequest<UserResponse>;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop).MustBeValidEmail();
        RuleFor(x => x.Password).MinimumLength(6).When(x => !string.IsNullOrEmpty(x.Password));
    }
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserResponse>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CreateUserHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<UserResponse> Handle(CreateUserCommand request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var email = request.Email.Trim();
        if (await db.Users.AnyAsync(u => u.Email == email, ct))
        {
            throw new ConflictException($"A user with the email '{email}' already exists.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            IsActive = true,
            PasswordHash = string.IsNullOrEmpty(request.Password)
                ? null
                : BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException($"A user with the email '{email}' already exists.");
        }

        return UserResponse.From(user);
    }
}
