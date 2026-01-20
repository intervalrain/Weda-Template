using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.Entities;

namespace Weda.Template.Application.Employees.Queries.GetEmployee;

public record GetEmployeeQuery(int Id) : IRequest<ErrorOr<Employee>>;
