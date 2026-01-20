using Riok.Mapperly.Abstractions;

using Weda.Template.Contracts.Employees;
using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Enums;
using Weda.Template.Domain.Employees.ValueObjects;

namespace Weda.Template.Api.Mapping;

[Mapper]
public static partial class EmployeeMapper
{
    [MapProperty(nameof(Employee.Name), nameof(EmployeeResponse.Name), Use = nameof(MapEmployeeName))]
    [MapProperty(nameof(Employee.Email), nameof(EmployeeResponse.Email), Use = nameof(MapEmail))]
    [MapProperty(nameof(Employee.Department), nameof(EmployeeResponse.Department), Use = nameof(MapDepartment))]
    [MapProperty(nameof(Employee.Status), nameof(EmployeeResponse.Status), Use = nameof(MapStatus))]
    public static partial EmployeeResponse ToResponse(Employee employee);

    public static IEnumerable<EmployeeResponse> ToResponseList(IEnumerable<Employee> employees)
        => employees.Select(ToResponse);

    private static string MapEmployeeName(EmployeeName name) => name.Value;

    private static string MapEmail(Email email) => email.Value;

    private static string MapDepartment(Department department) => department.ToString();

    private static string MapStatus(EmployeeStatus status) => status.ToString();
}
