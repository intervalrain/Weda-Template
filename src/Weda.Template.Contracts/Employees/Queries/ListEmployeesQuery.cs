using ErrorOr;

using Mediator;

using Weda.Template.Contracts.Employees.Dtos;

namespace Weda.Template.Contracts.Employees.Queries;

public record ListEmployeesQuery : IRequest<ErrorOr<List<EmployeeDto>>>;
