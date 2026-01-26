using Swashbuckle.AspNetCore.Filters;

namespace Weda.Template.Contracts.Employees.Requests;

/// <summary>
/// Request to update an existing employee.
/// </summary>
/// <param name="Name">Employee's full name.</param>
/// <param name="Email">Employee's email address.</param>
/// <param name="Department">Department the employee belongs to.</param>
/// <param name="Position">Job title or position.</param>
/// <param name="Status">Employee status (Active, OnLeave, Terminated).</param>
/// <param name="SupervisorId">ID of the employee's supervisor (optional).</param>
public record UpdateEmployeeRequest(
    string Name,
    string Email,
    string Department,
    string Position,
    string Status,
    int? SupervisorId);

public class UpdateEmployeeRequestExample : IExamplesProvider<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequest GetExamples() => new(
        Name: "John Doe",
        Email: "john.doe@example.com",
        Department: "Engineering",
        Position: "Senior Software Engineer",
        Status: "Active",
        SupervisorId: null);
}
