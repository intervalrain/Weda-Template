using Swashbuckle.AspNetCore.Filters;

namespace Weda.Template.Contracts.Employees.Requests;

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
