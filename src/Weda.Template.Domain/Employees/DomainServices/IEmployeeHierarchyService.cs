using ErrorOr;

using Weda.Template.Domain.Employees.Entities;

namespace Weda.Template.Domain.Employees.DomainServices;

/// <summary>
/// Domain service for handling employee hierarchy operations.
/// Validates supervisor assignments to prevent circular references.
/// </summary>
public interface IEmployeeHierarchyService
{
    /// <summary>
    /// Validates and assigns a supervisor to an employee.
    /// Checks for circular references in the hierarchy.
    /// </summary>
    /// <param name="employee">The employee to assign a supervisor to.</param>
    /// <param name="supervisorId">The supervisor's ID.</param>
    /// <returns>Success or validation errors.</returns>
    Task<ErrorOr<Success>> AssignSupervisorAsync(Employee employee, int? supervisorId);

    /// <summary>
    /// Gets the management chain (all supervisors) for an employee.
    /// </summary>
    /// <param name="employeeId">The employee's ID.</param>
    /// <returns>List of supervisors from immediate to top-level.</returns>
    Task<List<Employee>> GetManagementChainAsync(int employeeId);

    /// <summary>
    /// Gets all direct and indirect reports for a supervisor.
    /// </summary>
    /// <param name="supervisorId">The supervisor's ID.</param>
    /// <returns>List of all employees reporting to this supervisor.</returns>
    Task<List<Employee>> GetAllReportsAsync(int supervisorId);
}
