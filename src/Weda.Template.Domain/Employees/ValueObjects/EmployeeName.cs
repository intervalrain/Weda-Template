using ErrorOr;

using Weda.Template.Domain.Employees.Errors;

namespace Weda.Template.Domain.Employees.ValueObjects;

/// <summary>
/// Represents an employee's full name as a value object.
/// Ensures the name meets validation requirements.
/// </summary>
public sealed class EmployeeName : IEquatable<EmployeeName>
{
    /// <summary>
    /// Maximum allowed length for an employee name.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Gets the employee's full name.
    /// </summary>
    public string Value { get; }

    private EmployeeName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new EmployeeName value object after validation.
    /// </summary>
    /// <param name="value">The name string to validate and wrap.</param>
    /// <returns>A valid EmployeeName or validation errors.</returns>
    public static ErrorOr<EmployeeName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return EmployeeErrors.EmptyName;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            return EmployeeErrors.NameTooLong;
        }

        return new EmployeeName(trimmed);
    }

    public bool Equals(EmployeeName? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is EmployeeName other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(EmployeeName name)
    {
        return name.Value;
    }
}
