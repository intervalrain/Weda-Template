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
