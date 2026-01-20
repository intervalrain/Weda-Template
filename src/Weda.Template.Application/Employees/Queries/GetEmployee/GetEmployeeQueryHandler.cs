using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Errors;
using Weda.Template.Domain.Employees.Repositories;

namespace Weda.Template.Application.Employees.Queries.GetEmployee;

public class GetEmployeeQueryHandler(
    IEmployeeRepository _employeeRepository) : IRequestHandler<GetEmployeeQuery, ErrorOr<Employee>>
{
    public async ValueTask<ErrorOr<Employee>> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null)
        {
            return EmployeeErrors.NotFound;
        }

        return employee;
    }
}
