namespace Weda.Template.Contracts.Employees;

public record CreateEmployeeRequest(
    string Name,
    string Email,
    string Department,
    string Position,
    DateTime HireDate,
    Guid? SupervisorId);
