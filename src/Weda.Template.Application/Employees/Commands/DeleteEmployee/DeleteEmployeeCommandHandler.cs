using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.Errors;
using Weda.Template.Domain.Employees.Repositories;

namespace Weda.Template.Application.Employees.Commands.DeleteEmployee;

public class DeleteEmployeeCommandHandler(
    IEmployeeRepository _employeeRepository) : IRequestHandler<DeleteEmployeeCommand, ErrorOr<Deleted>>
{
    public async ValueTask<ErrorOr<Deleted>> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null)
        {
            return EmployeeErrors.NotFound;
        }

        var subordinates = await _employeeRepository.GetBySupervisorIdAsync(request.Id, cancellationToken);
        if (subordinates.Count > 0)
        {
            return EmployeeErrors.HasSubordinates;
        }

        await _employeeRepository.DeleteAsync(employee, cancellationToken);

        return Result.Deleted;
    }
}
