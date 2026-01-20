using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.Entities;

namespace Weda.Template.Application.Employees.Queries.ListEmployees;

public record ListEmployeesQuery : IRequest<ErrorOr<List<Employee>>>;
