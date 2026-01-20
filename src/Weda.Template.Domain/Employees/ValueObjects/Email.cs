using System.Text.RegularExpressions;

using ErrorOr;

using Weda.Template.Domain.Employees.Errors;

namespace Weda.Template.Domain.Employees.ValueObjects;

/// <summary>
/// Represents an email address as a value object.
/// Ensures the email is valid and properly formatted.
/// </summary>
public sealed partial class Email : IEquatable<Email>
{
    /// <summary>
    /// Maximum allowed length for an email address.
    /// </summary>
    public const int MaxLength = 256;

    /// <summary>
    /// Gets the email address string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new Email value object after validation.
    /// </summary>
    /// <param name="value">The email string to validate and wrap.</param>
    /// <returns>A valid Email or validation errors.</returns>
    public static ErrorOr<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return EmployeeErrors.EmptyEmail;
        }

        var trimmed = value.Trim().ToLowerInvariant();

        if (trimmed.Length > MaxLength)
        {
            return EmployeeErrors.EmailTooLong;
        }

        if (!EmailRegex().IsMatch(trimmed))
        {
            return EmployeeErrors.InvalidEmailFormat;
        }

        return new Email(trimmed);
    }

    public bool Equals(Email? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Email other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Email email)
    {
        return email.Value;
    }

    private Email(string value)
    {
        Value = value;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
