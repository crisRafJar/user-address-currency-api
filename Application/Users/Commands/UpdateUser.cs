using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Application.Common;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Users.Commands;

public record UpdateUserRequest(string Name, string Email, bool IsActive);

public record UpdateUserCommand(int Id, string Name, string Email, bool IsActive) : IRequest<UserResponse>;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop).MustBeValidEmail();
    }
}

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserResponse>
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public UpdateUserHandler(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<UserResponse> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, ct)
                   ?? throw new NotFoundException($"User with id {request.Id} was not found.");

        var email = request.Email.Trim();
        if (await db.Users.AnyAsync(u => u.Email == email && u.Id != request.Id, ct))
        {
            throw new ConflictException($"Another user with the email '{email}' already exists.");
        }

        user.Name = request.Name.Trim();
        user.Email = email;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException($"Another user with the email '{email}' already exists.");
        }

        return UserResponse.From(user);
    }
}
