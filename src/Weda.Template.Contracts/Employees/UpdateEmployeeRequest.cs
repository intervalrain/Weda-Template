namespace Weda.Template.Contracts.Employees;

public record UpdateEmployeeRequest(
    string Name,
    string Email,
    string Department,
    string Position,
    string Status,
    Guid? SupervisorId);
