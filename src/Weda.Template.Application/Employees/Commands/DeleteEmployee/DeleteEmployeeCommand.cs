using ErrorOr;

using Mediator;

namespace Weda.Template.Application.Employees.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(int Id) : IRequest<ErrorOr<Deleted>>;
