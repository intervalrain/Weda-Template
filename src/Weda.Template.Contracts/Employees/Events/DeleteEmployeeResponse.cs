using Swashbuckle.AspNetCore.Filters;

namespace Weda.Template.Contracts.Employees.Events;

/// <summary>
/// Response for employee deletion operation.
/// </summary>
/// <param name="Id">ID of the deleted employee.</param>
/// <param name="Success">Whether the deletion was successful.</param>
/// <param name="Message">Result message describing the outcome.</param>
public record DeleteEmployeeResponse(
    int Id,
    bool Success,
    string Message);

public class DeleteEmployeeResponseExample : IExamplesProvider<DeleteEmployeeResponse>
{
    public DeleteEmployeeResponse GetExamples() => new(
        Id: 1,
        Success: true,
        Message: "Successfully deleted");
}
