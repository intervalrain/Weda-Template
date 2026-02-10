using ErrorOr;
using Mediator;
using Weda.Template.Application.Employees.Mapping;
using Weda.Template.Contracts.Employees.Dtos;
using Weda.Template.Contracts.Employees.Queries;
using Weda.Template.Domain.Employees.DomainServices;

namespace Weda.Template.Application.Employees.Queries.GetSubordinates;

public class GetSubordinatesQueryHandler(EmployeeHierarchyManager hierarchyManager) : IRequestHandler<GetSubordinatesQuery, ErrorOr<List<EmployeeDto>>>
{
    public async ValueTask<ErrorOr<List<EmployeeDto>>> Handle(GetSubordinatesQuery request, CancellationToken cancellationToken)
    {
        var subordinatesResult = await hierarchyManager.GetAllReportsAsync(request.SupervisorId);

        if (subordinatesResult.IsError)
        {
            return subordinatesResult.Errors;
        }

        return EmployeeMapper.ToDtoList(subordinatesResult.Value);
    }
}
