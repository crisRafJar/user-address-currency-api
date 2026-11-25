using System.Text.RegularExpressions;
using FluentValidation;

namespace UserAddressCurrencyApi.Application.Common;

public static class EmailValidationExtensions
{
    private static readonly Regex EmailRegex = new(
        @"^(?!\.)(?!.*\.\.)[A-Za-z0-9!#$%&'*+/=?^_`{|}~.\-]+(?<!\.)@" +
        @"(?:[A-Za-z0-9](?:[A-Za-z0-9\-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(250));

    public const int MaxLength = 320;

    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > MaxLength)
        {
            return false;
        }

        var value = email.Trim();
        var at = value.IndexOf('@');
        if (at < 1 || at > 64)
        {
            return false;
        }

        try
        {
            return EmailRegex.IsMatch(value);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    public static IRuleBuilderOptions<T, string> MustBeValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("'Email' must not be empty.")
            .MaximumLength(MaxLength).WithMessage($"'Email' must not exceed {MaxLength} characters.")
            .Must(IsValidEmail).WithMessage("'Email' is not a valid email address (expected format: name@domain.com).");
    }
}
