using Weda.Template.Ddd.Domain;
using Weda.Template.Domain.Employees.Entities;

namespace Weda.Template.Domain.Employees.Repositories;

/// <summary>
/// Repository interface for Employee aggregate operations.
/// </summary>
public interface IEmployeeRepository : IRepository<Employee>
{
    /// <summary>
    /// Finds an employee by their email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The employee if found, otherwise null.</returns>
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all employees who report to a specific supervisor.
    /// </summary>
    /// <param name="supervisorId">The supervisor's ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of employees reporting to the supervisor.</returns>
    Task<List<Employee>> GetBySupervisorIdAsync(Guid supervisorId, CancellationToken cancellationToken = default);
}
