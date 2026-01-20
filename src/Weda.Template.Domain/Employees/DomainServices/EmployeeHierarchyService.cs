using ErrorOr;

using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Errors;
using Weda.Template.Domain.Employees.Repositories;

namespace Weda.Template.Domain.Employees.DomainServices;

/// <summary>
/// Implementation of the employee hierarchy domain service.
/// </summary>
public class EmployeeHierarchyService(IEmployeeRepository _employeeRepository) : IEmployeeHierarchyService
{
    public async Task<ErrorOr<Success>> AssignSupervisorAsync(Employee employee, Guid? supervisorId)
    {
        if (supervisorId is null)
        {
            return employee.AssignSupervisor(null);
        }

        var supervisor = await _employeeRepository.GetByIdAsync(supervisorId.Value);
        if (supervisor is null)
        {
            return EmployeeErrors.SupervisorNotFound;
        }

        if (await WouldCreateCircularReferenceAsync(employee.Id, supervisorId.Value))
        {
            return EmployeeErrors.CircularSupervisorReference;
        }

        return employee.AssignSupervisor(supervisorId);
    }

    public async Task<List<Employee>> GetManagementChainAsync(Guid employeeId)
    {
        var chain = new List<Employee>();
        var currentId = employeeId;

        while (true)
        {
            var employee = await _employeeRepository.GetByIdAsync(currentId);
            if (employee?.SupervisorId is null)
            {
                break;
            }

            var supervisor = await _employeeRepository.GetByIdAsync(employee.SupervisorId.Value);
            if (supervisor is null)
            {
                break;
            }

            chain.Add(supervisor);
            currentId = supervisor.Id;
        }

        return chain;
    }

    public async Task<List<Employee>> GetAllReportsAsync(Guid supervisorId)
    {
        var allReports = new List<Employee>();
        var directReports = await _employeeRepository.GetBySupervisorIdAsync(supervisorId);

        foreach (var report in directReports)
        {
            allReports.Add(report);
            var indirectReports = await GetAllReportsAsync(report.Id);
            allReports.AddRange(indirectReports);
        }

        return allReports;
    }

    private async Task<bool> WouldCreateCircularReferenceAsync(Guid employeeId, Guid potentialSupervisorId)
    {
        var currentId = potentialSupervisorId;
        var visited = new HashSet<Guid> { employeeId };

        while (true)
        {
            if (visited.Contains(currentId))
            {
                return true;
            }

            visited.Add(currentId);

            var current = await _employeeRepository.GetByIdAsync(currentId);
            if (current?.SupervisorId is null)
            {
                return false;
            }

            currentId = current.SupervisorId.Value;
        }
    }
}
