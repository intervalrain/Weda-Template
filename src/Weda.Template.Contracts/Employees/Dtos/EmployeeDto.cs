namespace Weda.Template.Contracts.Employees.Dtos;

public record EmployeeDto(
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