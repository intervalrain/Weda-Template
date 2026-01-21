using ErrorOr;

using Mediator;

using Weda.Template.Contracts.Employees.Dtos;

namespace Weda.Template.Contracts.Employees.Queries;

public record GetSubordinatesQuery(int SupervisorId) : IRequest<ErrorOr<List<EmployeeDto>>>;
