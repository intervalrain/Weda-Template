using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.Entities;

namespace Weda.Template.Application.Employees.Queries.GetSubordinates;

public record GetSubordinatesQuery(int SupervisorId) : IRequest<ErrorOr<List<Employee>>>;
