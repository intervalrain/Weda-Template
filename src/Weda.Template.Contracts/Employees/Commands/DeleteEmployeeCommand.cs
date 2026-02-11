using ErrorOr;
using Weda.Core.Application.Interfaces;

namespace Weda.Template.Contracts.Employees.Commands;

public record DeleteEmployeeCommand(int Id) : ICommand<ErrorOr<Deleted>>;
