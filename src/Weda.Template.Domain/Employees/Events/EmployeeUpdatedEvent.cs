using Weda.Template.Domain.Common;
using Weda.Template.Domain.Employees.Entities;

namespace Weda.Template.Domain.Employees.Events;

/// <summary>
/// Domain event raised when an employee's information is updated.
/// </summary>
public record EmployeeUpdatedEvent(Employee Employee) : IDomainEvent;
