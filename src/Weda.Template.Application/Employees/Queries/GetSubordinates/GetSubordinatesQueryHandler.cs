using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.DomainServices;
using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Errors;
using Weda.Template.Domain.Employees.Repositories;

namespace Weda.Template.Application.Employees.Queries.GetSubordinates;

public class GetSubordinatesQueryHandler(
    IEmployeeRepository _employeeRepository,
    IEmployeeHierarchyService _hierarchyService) : IRequestHandler<GetSubordinatesQuery, ErrorOr<List<Employee>>>
{
    public async ValueTask<ErrorOr<List<Employee>>> Handle(GetSubordinatesQuery request, CancellationToken cancellationToken)
    {
        var supervisor = await _employeeRepository.GetByIdAsync(request.SupervisorId, cancellationToken);
        if (supervisor is null)
        {
            return EmployeeErrors.NotFound;
        }

        var subordinates = await _hierarchyService.GetAllReportsAsync(request.SupervisorId);
        return subordinates;
    }
}
