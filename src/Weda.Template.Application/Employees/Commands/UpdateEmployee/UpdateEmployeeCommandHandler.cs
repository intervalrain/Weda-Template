using ErrorOr;

using Mediator;

using Weda.Template.Application.Employees.Mapping;
using Weda.Template.Contracts.Employees.Commands;
using Weda.Template.Contracts.Employees.Dtos;
using Weda.Template.Domain.Employees.DomainServices;
using Weda.Template.Domain.Employees.Errors;
using Weda.Template.Domain.Employees.Repositories;
using Weda.Template.Domain.Employees.ValueObjects;

namespace Weda.Template.Application.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    EmployeeHierarchyManager hierarchyManager) : IRequestHandler<UpdateEmployeeCommand, ErrorOr<EmployeeDto>>
{
    public async ValueTask<ErrorOr<EmployeeDto>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null)
        {
            return EmployeeErrors.NotFound(request.Id);
        }

        // Check for duplicate name (exclude current employee)
        var existingByName = await employeeRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingByName is not null && existingByName.Id != request.Id)
        {
            return EmployeeErrors.DuplicateName;
        }

        // Check for duplicate email (exclude current employee)
        var existingByEmail = await employeeRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail is not null && existingByEmail.Id != request.Id)
        {
            return EmployeeErrors.DuplicateEmail;
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

        var departmentResult = Department.Create(request.Department);
        if (departmentResult.IsError)
        {
            return departmentResult.Errors;
        }

        employee.UpdateDepartment(departmentResult.Value);
        employee.UpdatePosition(request.Position);
        employee.UpdateStatus(request.Status);

        if (request.SupervisorId != employee.SupervisorId)
        {
            var supervisorResult = await hierarchyManager.AssignSupervisorAsync(employee, request.SupervisorId);
            if (supervisorResult.IsError)
            {
                return supervisorResult.Errors;
            }
        }

        await employeeRepository.UpdateAsync(employee, cancellationToken);

        return EmployeeMapper.ToDto(employee);
    }
}
