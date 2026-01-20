using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Enums;

namespace Weda.Template.Application.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand(
    Guid Id,
    string Name,
    string Email,
    Department Department,
    string Position,
    EmployeeStatus Status,
    Guid? SupervisorId) : IRequest<ErrorOr<Employee>>;
