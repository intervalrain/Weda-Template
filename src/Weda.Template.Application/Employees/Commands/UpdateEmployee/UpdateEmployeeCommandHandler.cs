using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.DomainServices;
using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Errors;
using Weda.Template.Domain.Employees.Repositories;

namespace Weda.Template.Application.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandHandler(
    IEmployeeRepository _employeeRepository,
    IEmployeeHierarchyService _hierarchyService) : IRequestHandler<UpdateEmployeeCommand, ErrorOr<Employee>>
{
    public async ValueTask<ErrorOr<Employee>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null)
        {
            return EmployeeErrors.NotFound;
        }

        var nameResult = employee.UpdateName(request.Name);
        if (nameResult.IsError)
        {
            return nameResult.Errors;
        }

        var emailResult = employee.UpdateEmail(request.Email);
        if (emailResult.IsError)
        {
            return emailResult.Errors;
        }

        employee.UpdateDepartment(request.Department);
        employee.UpdatePosition(request.Position);
        employee.UpdateStatus(request.Status);

        if (request.SupervisorId != employee.SupervisorId)
        {
            var supervisorResult = await _hierarchyService.AssignSupervisorAsync(employee, request.SupervisorId);
            if (supervisorResult.IsError)
            {
                return supervisorResult.Errors;
            }
        }

        await _employeeRepository.UpdateAsync(employee, cancellationToken);

        return employee;
    }
}
