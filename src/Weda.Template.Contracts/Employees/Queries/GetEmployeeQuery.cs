using ErrorOr;

using Mediator;

using Weda.Template.Contracts.Employees.Dtos;

namespace Weda.Template.Contracts.Employees.Queries;

public record GetEmployeeQuery(int Id) : IRequest<ErrorOr<EmployeeDto>>;
