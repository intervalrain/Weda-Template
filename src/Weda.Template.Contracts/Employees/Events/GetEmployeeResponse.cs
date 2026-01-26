using Swashbuckle.AspNetCore.Filters;

namespace Weda.Template.Contracts.Employees.Events;

/// <summary>
/// Response containing employee details.
/// </summary>
/// <param name="Id">Unique employee identifier.</param>
/// <param name="Name">Employee's full name.</param>
/// <param name="Email">Employee's email address.</param>
/// <param name="Department">Department the employee belongs to.</param>
/// <param name="Position">Job title or position.</param>
/// <param name="Status">Current employee status.</param>
public record GetEmployeeResponse(
    int Id,
    string Name,
    string Email,
    string Department,
    string Position,
    string Status);

public class GetEmployeeRequestExample : IExamplesProvider<GetEmployeeResponse>
{
    public GetEmployeeResponse GetExamples() => new(
        Id: 1,
        Name: "John Doe",
        Email: "john.doe@example.com",
        Department: "Engineering",
        Position: "Software Engineer",
        Status: "active");
}
