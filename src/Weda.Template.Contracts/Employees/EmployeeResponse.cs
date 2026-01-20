using Swashbuckle.AspNetCore.Filters;

namespace Weda.Template.Contracts.Employees;

public record EmployeeResponse(
    Guid Id,
    string Name,
    string Email,
    string Department,
    string Position,
    DateTime HireDate,
    string Status,
    Guid? SupervisorId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class EmployeeResponseExample : IExamplesProvider<EmployeeResponse>
{
    public EmployeeResponse GetExamples() => new(
        Id: Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
        Name: "John Doe",
        Email: "john.doe@example.com",
        Department: "Engineering",
        Position: "Software Engineer",
        HireDate: new DateTime(2024, 1, 15),
        Status: "Active",
        SupervisorId: null,
        CreatedAt: new DateTime(2024, 1, 15, 9, 0, 0),
        UpdatedAt: null);
}
