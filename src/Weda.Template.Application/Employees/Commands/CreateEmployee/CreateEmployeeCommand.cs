using ErrorOr;

using Mediator;

using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Enums;

namespace Weda.Template.Application.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    string Name,
    string Email,
    Department Department,
    string Position,
    DateTime HireDate,
    Guid? SupervisorId) : IRequest<ErrorOr<Employee>>;
