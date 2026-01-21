using ErrorOr;

using Mediator;

using Weda.Template.Contracts.Employees.Dtos;
using Weda.Template.Domain.Employees.Enums;

namespace Weda.Template.Contracts.Employees.Commands;

public record CreateEmployeeCommand(
    string Name,
    string Email,
    Department Department,
    string Position,
    DateTime HireDate,
    int? SupervisorId) : IRequest<ErrorOr<EmployeeDto>>;
