using ErrorOr;

using Weda.Template.Domain.Employees.Errors;

namespace Weda.Template.Domain.Employees.ValueObjects;

/// <summary>
/// Represents a department as a value object.
/// Ensures the department is valid and properly formatted.
/// </summary>
public sealed partial class Department : IEquatable<Department>
{
    /// <summary>
    /// Maximum allowed length for an email address.
    /// </summary>
    public const int MaxLength = 32;

    /// <summary>
    /// Gets the department address string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new department value object after validation.
    /// </summary>
    /// <param name="value">The department string to validate and wrap.</param>
    /// <returns>A valid department or validation errors.</returns>
    public static ErrorOr<Department> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return EmployeeErrors.EmptyDepartment;
        }

        var trimmed = value.Trim().ToLowerInvariant();

        if (trimmed.Length > MaxLength)
        {
            return EmployeeErrors.DepartmentTooLong;
        }

        return new Department(trimmed);
    }

    public bool Equals(Department? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Department other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Department department)
    {
        return department.Value;
    }

    private Department(string value)
    {
        Value = value;
    }
}
