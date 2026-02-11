using ErrorOr;
using Weda.Core.Application.Interfaces;
using Weda.Template.Contracts.Employees.Dtos;

namespace Weda.Template.Contracts.Employees.Commands;

public record CreateEmployeeCommand(
    string Name,
    string Email,
    string Department,
    string Position,
    DateTime HireDate,
    int? SupervisorId = null) : ICommand<ErrorOr<EmployeeDto>>;
