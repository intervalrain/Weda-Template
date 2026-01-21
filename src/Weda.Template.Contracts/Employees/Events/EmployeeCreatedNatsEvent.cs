namespace Weda.Template.Contracts.Employees.Events;

public record EmployeeCreatedNatsEvent(
    int Id,
    string Name,
    string Email,
    string Department,
    string Position,
    DateTime CreatedAt);
