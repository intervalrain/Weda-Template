using ErrorOr;
using Weda.Core.Domain;
using Weda.Template.Domain.Employees.Enums;
using Weda.Template.Domain.Employees.Errors;
using Weda.Template.Domain.Employees.Events;
using Weda.Template.Domain.Employees.ValueObjects;

namespace Weda.Template.Domain.Employees.Entities;

/// <summary>
/// Represents an employee in the organization.
/// This is the aggregate root for the Employee bounded context.
/// </summary>
public class Employee : AggregateRoot<int>
{
    /// <summary>
    /// Gets the employee's full name.
    /// </summary>
    public EmployeeName Name { get; private set; }

    /// <summary>
    /// Gets the employee's email address.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Gets the department the employee belongs to.
    /// </summary>
    public Department Department { get; private set; }

    /// <summary>
    /// Gets the employee's job position/title.
    /// </summary>
    public string Position { get; private set; }

    /// <summary>
    /// Gets the date when the employee was hired.
    /// </summary>
    public DateTime HireDate { get; private set; }

    /// <summary>
    /// Gets the current employment status.
    /// </summary>
    public EmployeeStatus Status { get; private set; }

    /// <summary>
    /// Gets the ID of the employee's direct supervisor.
    /// Null indicates the employee has no supervisor (e.g., CEO).
    /// </summary>
    public int? SupervisorId { get; private set; }

    /// <summary>
    /// Gets the date and time when this record was created.
    /// </summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>
    /// Gets the date and time when this record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    private Employee(
        EmployeeName name,
        Email email,
        Department department,
        string position,
        DateTime hireDate,
        int? supervisorId,
        DateTime createdAt)
    {
        Name = name;
        Email = email;
        Department = department;
        Position = position;
        HireDate = hireDate;
        SupervisorId = supervisorId;
        Status = EmployeeStatus.Active;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Creates a new Employee instance.
    /// </summary>
    public static ErrorOr<Employee> Create(
        string name,
        string email,
        Department department,
        string position,
        DateTime hireDate,
        int? supervisorId = null,
        DateTime? createdAt = null)
    {
        var nameResult = EmployeeName.Create(name);
        if (nameResult.IsError)
        {
            return nameResult.Errors;
        }

        var emailResult = Email.Create(email);
        if (emailResult.IsError)
        {
            return emailResult.Errors;
        }

        if (string.IsNullOrWhiteSpace(position))
        {
            return EmployeeErrors.EmptyPosition;
        }

        if (position.Trim().Length > 100)
        {
            return EmployeeErrors.PositionTooLong;
        }

        var employee = new Employee(
            nameResult.Value,
            emailResult.Value,
            department,
            position.Trim(),
            hireDate,
            supervisorId,
            createdAt ?? DateTime.UtcNow);

        employee.RaiseDomainEvent(new EmployeeCreatedEvent(employee));

        return employee;
    }

    /// <summary>
    /// Updates the employee's name.
    /// </summary>
    public ErrorOr<Success> UpdateName(string newName)
    {
        if (Status == EmployeeStatus.Inactive)
        {
            return EmployeeErrors.CannotModifyInactiveEmployee;
        }

        var nameResult = EmployeeName.Create(newName);
        if (nameResult.IsError)
        {
            return nameResult.Errors;
        }

        Name = nameResult.Value;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }

    /// <summary>
    /// Updates the employee's email.
    /// </summary>
    public ErrorOr<Success> UpdateEmail(string newEmail)
    {
        if (Status == EmployeeStatus.Inactive)
        {
            return EmployeeErrors.CannotModifyInactiveEmployee;
        }

        var emailResult = Email.Create(newEmail);
        if (emailResult.IsError)
        {
            return emailResult.Errors;
        }

        Email = emailResult.Value;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }

    /// <summary>
    /// Updates the employee's department.
    /// </summary>
    public ErrorOr<Success> UpdateDepartment(Department newDepartment)
    {
        if (Status == EmployeeStatus.Inactive)
        {
            return EmployeeErrors.CannotModifyInactiveEmployee;
        }

        Department = newDepartment;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }

    /// <summary>
    /// Updates the employee's position.
    /// </summary>
    public ErrorOr<Success> UpdatePosition(string newPosition)
    {
        if (Status == EmployeeStatus.Inactive)
        {
            return EmployeeErrors.CannotModifyInactiveEmployee;
        }

        if (string.IsNullOrWhiteSpace(newPosition))
        {
            return EmployeeErrors.EmptyPosition;
        }

        if (newPosition.Trim().Length > 100)
        {
            return EmployeeErrors.PositionTooLong;
        }

        Position = newPosition.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }

    /// <summary>
    /// Assigns a supervisor to this employee.
    /// </summary>
    public ErrorOr<Success> AssignSupervisor(int? supervisorId)
    {
        if (Status == EmployeeStatus.Inactive)
        {
            return EmployeeErrors.CannotModifyInactiveEmployee;
        }

        if (supervisorId == Id)
        {
            return EmployeeErrors.CannotBeSelfSupervisor;
        }

        SupervisorId = supervisorId;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new SupervisorAssignedEvent(this, supervisorId));

        return Result.Success;
    }

    /// <summary>
    /// Updates the employee's status.
    /// </summary>
    public ErrorOr<Success> UpdateStatus(EmployeeStatus newStatus)
    {
        if (Status == EmployeeStatus.Inactive && newStatus != EmployeeStatus.Active)
        {
            return EmployeeErrors.CannotModifyInactiveEmployee;
        }

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }

    /// <summary>
    /// Sets the employee status to on leave.
    /// </summary>
    public ErrorOr<Success> SetOnLeave()
    {
        if (Status == EmployeeStatus.Inactive)
        {
            return EmployeeErrors.CannotModifyInactiveEmployee;
        }

        Status = EmployeeStatus.OnLeave;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }

    /// <summary>
    /// Reactivates an employee from on leave status.
    /// </summary>
    public ErrorOr<Success> Activate()
    {
        if (Status == EmployeeStatus.Active)
        {
            return EmployeeErrors.AlreadyActive;
        }

        if (Status == EmployeeStatus.Inactive)
        {
            return EmployeeErrors.CannotModifyInactiveEmployee;
        }

        Status = EmployeeStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }

    /// <summary>
    /// Deactivates the employee (termination or resignation).
    /// </summary>
    public ErrorOr<Success> Deactivate()
    {
        if (Status == EmployeeStatus.Inactive)
        {
            return EmployeeErrors.AlreadyInactive;
        }

        Status = EmployeeStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success;
    }

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private Employee()
    {
        Name = null!;
        Email = null!;
        Position = null!;
    }
}
