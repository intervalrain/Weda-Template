using ErrorOr;

using Mediator;

using Weda.Template.Application.Employees.Mapping;
using Weda.Template.Contracts.Employees.Commands;
using Weda.Template.Contracts.Employees.Dtos;
using Weda.Template.Domain.Employees.DomainServices;
using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Repositories;
using Weda.Template.Domain.Employees.ValueObjects;

namespace Weda.Template.Application.Employees.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    EmployeeHierarchyManager hierarchyManager) : IRequestHandler<CreateEmployeeCommand, ErrorOr<EmployeeDto>>
{
    public async ValueTask<ErrorOr<EmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var departmentResult = Department.Create(request.Department);
        if (departmentResult.IsError)
        {
            return departmentResult.Errors;
        }

        var employeeResult = Employee.Create(
            request.Name,
            request.Email,
            departmentResult.Value,
            request.Position,
            request.HireDate);

        if (employeeResult.IsError)
        {
            return employeeResult.Errors;
        }

        var employee = employeeResult.Value;

        if (request.SupervisorId.HasValue)
        {
            var assignResult = await hierarchyManager.AssignSupervisorAsync(employee, request.SupervisorId);
            if (assignResult.IsError)
            {
                return assignResult.Errors;
            }
        }

        await employeeRepository.AddAsync(employee, cancellationToken);

        return EmployeeMapper.ToDto(employee);
    }
}
