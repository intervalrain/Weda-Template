using ErrorOr;

using Mediator;

using Weda.Template.Application.Employees.Mapping;
using Weda.Template.Contracts.Employees.Dtos;
using Weda.Template.Contracts.Employees.Queries;
using Weda.Template.Domain.Employees.Errors;
using Weda.Template.Domain.Employees.Repositories;

namespace Weda.Template.Application.Employees.Queries.GetEmployee;

public class GetEmployeeQueryHandler(
    IEmployeeRepository employeeRepository) : IRequestHandler<GetEmployeeQuery, ErrorOr<EmployeeDto>>
{
    public async ValueTask<ErrorOr<EmployeeDto>> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null)
        {
            return EmployeeErrors.NotFound;
        }

        return EmployeeMapper.ToDto(employee);
    }
}
