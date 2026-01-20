using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Enums;

namespace Weda.Template.Application.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand(
    int Id,
    string Name,
    string Email,
    Department Department,
    string Position,
    EmployeeStatus Status,
    int? SupervisorId) : IRequest<ErrorOr<Employee>>;
