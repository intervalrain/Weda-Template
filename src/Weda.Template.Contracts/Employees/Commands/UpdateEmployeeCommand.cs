using ErrorOr;

using Mediator;

using Weda.Template.Contracts.Employees.Dtos;
using Weda.Template.Domain.Employees.Enums;

namespace Weda.Template.Contracts.Employees.Commands;

public record UpdateEmployeeCommand(
    int Id,
    string Name,
    string Email,
    string Department,
    string Position,
    EmployeeStatus Status,
    int? SupervisorId) : IRequest<ErrorOr<EmployeeDto>>;
