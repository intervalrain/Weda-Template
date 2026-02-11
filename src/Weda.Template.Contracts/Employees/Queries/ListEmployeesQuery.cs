using ErrorOr;
using Weda.Core.Application.Interfaces;
using Weda.Template.Contracts.Employees.Dtos;

namespace Weda.Template.Contracts.Employees.Queries;

public record ListEmployeesQuery : IQuery<ErrorOr<List<EmployeeDto>>>;
