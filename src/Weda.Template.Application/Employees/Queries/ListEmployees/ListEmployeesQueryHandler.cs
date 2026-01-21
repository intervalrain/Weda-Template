using ErrorOr;

using Mediator;

using Weda.Template.Application.Employees.Mapping;
using Weda.Template.Contracts.Employees.Dtos;
using Weda.Template.Contracts.Employees.Queries;
using Weda.Template.Domain.Employees.Repositories;

namespace Weda.Template.Application.Employees.Queries.ListEmployees;

public class ListEmployeesQueryHandler(
    IEmployeeRepository employeeRepository) : IRequestHandler<ListEmployeesQuery, ErrorOr<List<EmployeeDto>>>
{
    public async ValueTask<ErrorOr<List<EmployeeDto>>> Handle(ListEmployeesQuery request, CancellationToken cancellationToken)
    {
        var employees = await employeeRepository.GetAllAsync(cancellationToken);
        return EmployeeMapper.ToDtoList(employees);
    }
}
