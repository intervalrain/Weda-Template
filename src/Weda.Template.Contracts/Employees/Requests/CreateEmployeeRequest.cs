using Swashbuckle.AspNetCore.Filters;

namespace Weda.Template.Contracts.Employees.Requests;

/// <summary>
/// Request to create a new employee.
/// </summary>
/// <param name="Name">Employee's full name.</param>
/// <param name="Email">Employee's email address.</param>
/// <param name="Department">Department the employee belongs to.</param>
/// <param name="Position">Job title or position.</param>
/// <param name="HireDate">Date when the employee was hired.</param>
/// <param name="SupervisorId">ID of the employee's supervisor (optional).</param>
public record CreateEmployeeRequest(
    string Name,
    string Email,
    string Department,
    string Position,
    DateTime HireDate,
    int? SupervisorId);

public class CreateEmployeeRequestExample : IExamplesProvider<CreateEmployeeRequest>
{
    public CreateEmployeeRequest GetExamples() => new(
        Name: "John Doe",
        Email: "john.doe@example.com",
        Department: "Engineering",
        Position: "Software Engineer",
        HireDate: new DateTime(2024, 1, 15),
        SupervisorId: null);
}
