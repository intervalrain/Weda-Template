using ErrorOr;

using Mediator;

namespace Weda.Template.Contracts.Employees.Commands;

public record DeleteEmployeeCommand(int Id) : IRequest<ErrorOr<Deleted>>;
