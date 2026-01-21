namespace Weda.Template.Contracts.Employees.Events;

public record GetEmployeeResponse(
    int Id,
    string Name,
    string Email,
    string Department,
    string Position,
    string Status);
