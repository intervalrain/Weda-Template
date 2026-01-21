namespace Weda.Template.Contracts.Employees.Events;

public record EmployeeStatusChangedNatsEvent(
    int Id,
    string PreviousStatus,
    string NewStatus,
    DateTime ChangedAt);
