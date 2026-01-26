using Swashbuckle.AspNetCore.Filters;

namespace Weda.Template.Contracts.Employees.Events;

/// <summary>
/// Request containing employee details.
/// </summary>
/// <param name="Name">Employee's full name.</param>
/// <param name="Email">Employee's email address.</param>
/// <param name="Department">Department the employee belongs to.</param>
/// <param name="Position">Job title or position.</param>
public record CreateEmployeeNatsEvent(
    string Name,
    string Email,
    string Department,
    string Position);

public class CreateEmployeeNatsEventExample : IExamplesProvider<CreateEmployeeNatsEvent>
{
    public CreateEmployeeNatsEvent GetExamples() => new(
        Name: "John Doe",
        Email: "john.doe@example.com",
        Department: "engineering",
        Position: "Software Engineer");
}
