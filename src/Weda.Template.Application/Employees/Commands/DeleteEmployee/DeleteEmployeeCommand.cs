using ErrorOr;

using Mediator;

namespace Weda.Template.Application.Employees.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;
