using Swashbuckle.AspNetCore.Filters;

namespace Weda.Template.Contracts.Employees.Events;

/// <summary>
/// Response containing a list of employees.
/// </summary>
/// <param name="Employees">List of employee details.</param>
public record ListEmployeesResponse(List<GetEmployeeResponse> Employees);

public class ListEmployeesResponseExample : IExamplesProvider<ListEmployeesResponse>
{
    public ListEmployeesResponse GetExamples() => new([
        new GetEmployeeResponse(
            Id: 1,
            Name: "John Doe",
            Email: "john.doe@example.com",
            Department: "Engineering",
            Position: "Software Engineer",
            Status: "active"),
        new GetEmployeeResponse(
            Id: 2,
            Name: "Rain Hu",
            Email: "rain.hu@example.com",
            Department: "Engineering",
            Position: "Software Engineer",
            Status: "active")
        ]);
}
