using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Repositories;

namespace Weda.Template.Application.Employees.Queries.ListEmployees;

public class ListEmployeesQueryHandler(
    IEmployeeRepository _employeeRepository) : IRequestHandler<ListEmployeesQuery, ErrorOr<List<Employee>>>
{
    public async ValueTask<ErrorOr<List<Employee>>> Handle(ListEmployeesQuery request, CancellationToken cancellationToken)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        return employees;
    }
}
