using ErrorOr;
using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Errors;
using Weda.Template.Domain.Employees.Repositories;

namespace Weda.Template.Domain.Employees.DomainServices;

/// <summary>
/// Domain service for managing employee hierarchy operations.
/// </summary>
public class EmployeeHierarchyManager(IEmployeeRepository employeeRepository)
{
    public async Task<ErrorOr<Success>> AssignSupervisorAsync(Employee employee, int? supervisorId)
    {
        if (supervisorId is null)
        {
            return employee.AssignSupervisor(null);
        }

        var supervisor = await employeeRepository.GetByIdAsync(supervisorId.Value);
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

    public async Task<ErrorOr<IReadOnlyList<Employee>>> GetManagementChainAsync(int employeeId)
    {
        var employee = await employeeRepository.GetByIdAsync(employeeId);
        if (employee is null)
        {
            return EmployeeErrors.NotFound(employeeId);
        }

        var chain = new List<Employee>();
        var currentId = employeeId;

        while (true)
        {
            var current = await employeeRepository.GetByIdAsync(currentId);
            if (current?.SupervisorId is null)
            {
                break;
            }

            var supervisor = await employeeRepository.GetByIdAsync(current.SupervisorId.Value);
            if (supervisor is null)
            {
                break;
            }

            chain.Add(supervisor);
            currentId = supervisor.Id;
        }

        return chain;
    }

    public async Task<ErrorOr<IReadOnlyList<Employee>>> GetAllReportsAsync(int supervisorId)
    {
        var supervisor = await employeeRepository.GetByIdAsync(supervisorId);
        if (supervisor is null)
        {
            return EmployeeErrors.NotFound(supervisorId);
        }

        return (await CollectReportAsync(supervisorId)).ToErrorOr();
    }

    private async Task<IReadOnlyList<Employee>> CollectReportAsync(int supervisorId)
    {
        var allReports = new List<Employee>();
        var directReports = await employeeRepository.GetBySupervisorIdAsync(supervisorId);

        foreach (var report in directReports)
        {
            allReports.Add(report);
            var indirectReports = await CollectReportAsync(report.Id);
            allReports.AddRange(indirectReports);
        }

        return allReports;
    }

    private async Task<bool> WouldCreateCircularReferenceAsync(int employeeId, int potentialSupervisorId)
    {
        var currentId = potentialSupervisorId;
        var visited = new HashSet<int> { employeeId };

        while (true)
        {
            if (visited.Contains(currentId))
            {
                return true;
            }

            visited.Add(currentId);

            var current = await employeeRepository.GetByIdAsync(currentId);
            if (current?.SupervisorId is null)
            {
                return false;
            }

            currentId = current.SupervisorId.Value;
        }
    }
}
