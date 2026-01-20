using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.Entities;

namespace Weda.Template.Application.Employees.Queries.GetEmployee;

public record GetEmployeeQuery(Guid Id) : IRequest<ErrorOr<Employee>>;
