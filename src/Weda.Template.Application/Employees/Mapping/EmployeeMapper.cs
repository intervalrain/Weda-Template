using Riok.Mapperly.Abstractions;

using Weda.Template.Contracts.Employees.Dtos;
using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Enums;
using Weda.Template.Domain.Employees.ValueObjects;

namespace Weda.Template.Application.Employees.Mapping;

[Mapper]
public static partial class EmployeeMapper
{
    [MapProperty(nameof(Employee.Name), nameof(EmployeeDto.Name), Use = nameof(MapEmployeeName))]
    [MapProperty(nameof(Employee.Email), nameof(EmployeeDto.Email), Use = nameof(MapEmail))]
    [MapProperty(nameof(Employee.Department), nameof(EmployeeDto.Department), Use = nameof(MapDepartment))]
    [MapProperty(nameof(Employee.Status), nameof(EmployeeDto.Status), Use = nameof(MapStatus))]
    public static partial EmployeeDto ToDto(Employee employee);

    public static List<EmployeeDto> ToDtoList(IEnumerable<Employee> employees)
        => employees.Select(ToDto).ToList();

    private static string MapEmployeeName(EmployeeName name) => name.Value;

    private static string MapEmail(Email email) => email.Value;

    private static string MapDepartment(Department department) => department.Value;

    private static string MapStatus(EmployeeStatus status) => status.ToString();
}
