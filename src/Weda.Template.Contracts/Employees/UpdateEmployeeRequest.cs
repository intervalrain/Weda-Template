using Swashbuckle.AspNetCore.Filters;

namespace Weda.Template.Contracts.Employees;

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
