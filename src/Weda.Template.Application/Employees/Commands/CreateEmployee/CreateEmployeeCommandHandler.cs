using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.DomainServices;
using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Repositories;

namespace Weda.Template.Application.Employees.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler(
    IEmployeeRepository _employeeRepository,
    IEmployeeHierarchyService _hierarchyService) : IRequestHandler<CreateEmployeeCommand, ErrorOr<Employee>>
{
    public async ValueTask<ErrorOr<Employee>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employeeResult = Employee.Create(
            request.Name,
            request.Email,
            request.Department,
            request.Position,
            request.HireDate);

        if (employeeResult.IsError)
        {
            return employeeResult.Errors;
        }

        var employee = employeeResult.Value;

        if (request.SupervisorId.HasValue)
        {
            var assignResult = await _hierarchyService.AssignSupervisorAsync(employee, request.SupervisorId);
            if (assignResult.IsError)
            {
                return assignResult.Errors;
            }
        }

        await _employeeRepository.AddAsync(employee, cancellationToken);

        return employee;
    }
}
