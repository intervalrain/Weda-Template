namespace Weda.Template.Domain.Employees.Enums;

/// <summary>
/// Represents the employment status of an employee.
/// </summary>
public enum EmployeeStatus
{
    /// <summary>
    /// Employee is currently active and working.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Employee is on leave (vacation, sick, parental, etc.).
    /// </summary>
    OnLeave = 1,

    /// <summary>
    /// Employee has been terminated or resigned.
    /// </summary>
    Inactive = 2,
}
