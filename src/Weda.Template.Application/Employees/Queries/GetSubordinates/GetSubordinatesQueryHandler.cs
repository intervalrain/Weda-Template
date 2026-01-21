using ErrorOr;

using Mediator;

using Weda.Template.Application.Employees.Mapping;
using Weda.Template.Contracts.Employees.Dtos;
using Weda.Template.Contracts.Employees.Queries;
using Weda.Template.Domain.Employees.DomainServices;
using Weda.Template.Domain.Employees.Errors;
using Weda.Template.Domain.Employees.Repositories;

namespace Weda.Template.Application.Employees.Queries.GetSubordinates;

public class GetSubordinatesQueryHandler(
    IEmployeeRepository employeeRepository,
    IEmployeeHierarchyService hierarchyService) : IRequestHandler<GetSubordinatesQuery, ErrorOr<List<EmployeeDto>>>
{
    public async ValueTask<ErrorOr<List<EmployeeDto>>> Handle(GetSubordinatesQuery request, CancellationToken cancellationToken)
    {
        var supervisor = await employeeRepository.GetByIdAsync(request.SupervisorId, cancellationToken);
        if (supervisor is null)
        {
            return EmployeeErrors.NotFound;
        }

        var subordinates = await hierarchyService.GetAllReportsAsync(request.SupervisorId);
        return EmployeeMapper.ToDtoList(subordinates);
    }
}
