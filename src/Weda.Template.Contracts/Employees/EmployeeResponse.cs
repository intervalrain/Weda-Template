using Swashbuckle.AspNetCore.Filters;

namespace Weda.Template.Contracts.Employees;

public record EmployeeResponse(
    int Id,
    string Name,
    string Email,
    string Department,
    string Position,
    DateTime HireDate,
    string Status,
    int? SupervisorId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class EmployeeResponseExample : IExamplesProvider<EmployeeResponse>
{
    public EmployeeResponse GetExamples() => new(
        Id: 1,
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

public class EmployeeResponseListExample : IExamplesProvider<IEnumerable<EmployeeResponse>>
{
    public IEnumerable<EmployeeResponse> GetExamples() =>
    [
        new(
            Id: 1,
            Name: "John Doe",
            Email: "john.doe@example.com",
            Department: "Engineering",
            Position: "Software Engineer",
            HireDate: new DateTime(2024, 1, 15),
            Status: "Active",
            SupervisorId: null,
            CreatedAt: new DateTime(2024, 1, 15, 9, 0, 0),
            UpdatedAt: null),
        new(
            Id: 2,
            Name: "Jane Smith",
            Email: "jane.smith@example.com",
            Department: "Engineering",
            Position: "Tech Lead",
            HireDate: new DateTime(2023, 6, 1),
            Status: "Active",
            SupervisorId: null,
            CreatedAt: new DateTime(2023, 6, 1, 9, 0, 0),
            UpdatedAt: new DateTime(2024, 1, 10, 14, 30, 0)),
    ];
}
