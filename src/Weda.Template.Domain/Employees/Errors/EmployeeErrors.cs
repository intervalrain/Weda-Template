using ErrorOr;

namespace Weda.Template.Domain.Employees.Errors;

/// <summary>
/// Contains all domain-specific errors for the Employee aggregate.
/// </summary>
public static class EmployeeErrors
{
    // Not Found Errors
    public static Error NotFound(int id) => Error.NotFound(
        code: "Employee.NotFound",
        description: $"The employee with the ID {id} was not found.");

    public static readonly Error SupervisorNotFound = Error.NotFound(
        code: "Employee.SupervisorNotFound",
        description: "The specified supervisor was not found.");

    // Validation Errors - Name
    public static readonly Error EmptyName = Error.Validation(
        code: "Employee.EmptyName",
        description: "Employee name cannot be empty.");

    public static readonly Error NameTooLong = Error.Validation(
        code: "Employee.NameTooLong",
        description: "Employee name cannot exceed 100 characters.");

    // Validation Errors - Email
    public static readonly Error EmptyEmail = Error.Validation(
        code: "Employee.EmptyEmail",
        description: "Employee email cannot be empty.");

    public static readonly Error EmailTooLong = Error.Validation(
        code: "Employee.EmailTooLong",
        description: "Employee email cannot exceed 256 characters.");

    public static readonly Error InvalidEmailFormat = Error.Validation(
        code: "Employee.InvalidEmailFormat",
        description: "The email format is invalid.");

    public static readonly Error DuplicateEmail = Error.Conflict(
        code: "Employee.DuplicateEmail",
        description: "An employee with this email already exists.");

    public static readonly Error DuplicateName = Error.Conflict(
        code: "Employee.DuplicateName",
        description: "An employee with this name already exists.");

    // Validation Errors - Department
    public static readonly Error EmptyDepartment = Error.Validation(
        code: "Employee.EmptyDepartment",
        description: "Employee department cannot be empty.");

    public static readonly Error DepartmentTooLong = Error.Validation(
        code: "Employee.DepartmentTooLong",
        description: "Employee department cannot exceed 32 characters.");

    // Validation Errors - Position
    public static readonly Error EmptyPosition = Error.Validation(
        code: "Employee.EmptyPosition",
        description: "Employee position cannot be empty.");

    public static readonly Error PositionTooLong = Error.Validation(
        code: "Employee.PositionTooLong",
        description: "Employee position cannot exceed 100 characters.");

    // Business Rule Errors
    public static readonly Error CannotModifyInactiveEmployee = Error.Validation(
        code: "Employee.CannotModifyInactive",
        description: "Cannot modify an inactive employee.");

    public static readonly Error CannotBeSelfSupervisor = Error.Validation(
        code: "Employee.CannotBeSelfSupervisor",
        description: "An employee cannot be their own supervisor.");

    public static readonly Error CircularSupervisorReference = Error.Validation(
        code: "Employee.CircularSupervisorReference",
        description: "Assigning this supervisor would create a circular reference.");

    public static readonly Error AlreadyActive = Error.Conflict(
        code: "Employee.AlreadyActive",
        description: "The employee is already active.");

    public static readonly Error AlreadyInactive = Error.Conflict(
        code: "Employee.AlreadyInactive",
        description: "The employee is already inactive.");

    public static readonly Error HasSubordinates = Error.Conflict(
        code: "Employee.HasSubordinates",
        description: "Cannot delete an employee who has subordinates. Please reassign or remove subordinates first.");
}
