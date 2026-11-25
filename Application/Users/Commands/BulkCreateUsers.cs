using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Domain.Entities;
using UserAddressCurrencyApi.Infrastructure.Data;

namespace UserAddressCurrencyApi.Application.Users.Commands;

public record BulkUserItem(string Name, string Email, string? Password = null);

public record BulkCreateUsersCommand(List<BulkUserItem> Users) : IRequest<BulkCreateUsersResult>;

public record BulkUserFailure(int Index, string? Email, IReadOnlyList<string> Reasons);

public record BulkCreateUsersResult(
    int Total,
    int Created,
    int Failed,
    IReadOnlyList<UserResponse> CreatedUsers,
    IReadOnlyList<BulkUserFailure> Failures);

public class BulkCreateUsersValidator : AbstractValidator<BulkCreateUsersCommand>
{
    public const int MaxUsers = 1000;

    public BulkCreateUsersValidator()
    {
        RuleFor(x => x.Users)
            .NotNull().WithMessage("'users' is required.")
            .NotEmpty().WithMessage("'users' must contain at least one user.")
            .Must(u => u == null || u.Count <= MaxUsers)
            .WithMessage($"'users' must not contain more than {MaxUsers} items.");

        RuleForEach(x => x.Users)
            .NotNull().WithMessage("Null items are not allowed in 'users'.");
    }
}

public class BulkCreateUsersHandler : IRequestHandler<BulkCreateUsersCommand, BulkCreateUsersResult>
{
    private const int MaxDegreeOfParallelism = 8;

    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IValidator<CreateUserCommand> _itemValidator;

    public BulkCreateUsersHandler(IDbContextFactory<AppDbContext> factory, IValidator<CreateUserCommand> itemValidator)
    {
        _factory = factory;
        _itemValidator = itemValidator;
    }

    public async Task<BulkCreateUsersResult> Handle(BulkCreateUsersCommand request, CancellationToken ct)
    {
        using var gate = new SemaphoreSlim(MaxDegreeOfParallelism);

        var tasks = request.Users
            .Select((item, index) => ProcessItemAsync(item, index, gate, ct))
            .ToList();

        var outcomes = await Task.WhenAll(tasks);

        var created = outcomes.Where(o => o.User is not null).Select(o => o.User!).ToList();
        var failures = outcomes.Where(o => o.Failure is not null).Select(o => o.Failure!).ToList();

        return new BulkCreateUsersResult(outcomes.Length, created.Count, failures.Count, created, failures);
    }

    private async Task<(UserResponse? User, BulkUserFailure? Failure)> ProcessItemAsync(
        BulkUserItem item, int index, SemaphoreSlim gate, CancellationToken ct)
    {
        await gate.WaitAsync(ct);
        try
        {
            var validation = await _itemValidator.ValidateAsync(
                new CreateUserCommand(item.Name, item.Email, item.Password), ct);

            if (!validation.IsValid)
            {
                var reasons = validation.Errors.Select(e => e.ErrorMessage).ToList();
                return (null, new BulkUserFailure(index, item.Email, reasons));
            }

            await using var db = await _factory.CreateDbContextAsync(ct);

            var email = item.Email.Trim();
            if (await db.Users.AnyAsync(u => u.Email == email, ct))
            {
                return (null, Fail(index, email, $"A user with the email '{email}' already exists."));
            }

            var user = new User
            {
                Name = item.Name.Trim(),
                Email = email,
                IsActive = true,
                PasswordHash = string.IsNullOrEmpty(item.Password)
                    ? null
                    : BCrypt.Net.BCrypt.HashPassword(item.Password)
            };

            db.Users.Add(user);

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                return (null, Fail(index, email, $"A user with the email '{email}' already exists."));
            }

            return (UserResponse.From(user), null);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return (null, Fail(index, item.Email, $"Unexpected error: {ex.Message}"));
        }
        finally
        {
            gate.Release();
        }
    }

    private static BulkUserFailure Fail(int index, string? email, string reason) =>
        new(index, email, new[] { reason });
}
