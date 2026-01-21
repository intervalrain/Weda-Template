using Riok.Mapperly.Abstractions;

using Weda.Template.Contracts.Employees;
using Weda.Template.Contracts.Employees.Dtos;

namespace Weda.Template.Api.Mapping;

[Mapper]
public static partial class EmployeeMapper
{
    public static partial EmployeeResponse ToResponse(EmployeeDto dto);

    public static IEnumerable<EmployeeResponse> ToResponseList(IEnumerable<EmployeeDto> dtos)
        => dtos.Select(ToResponse);
}
