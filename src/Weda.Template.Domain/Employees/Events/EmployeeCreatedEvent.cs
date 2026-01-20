using Weda.Template.Domain.Common;
using Weda.Template.Domain.Employees.Entities;

namespace Weda.Template.Domain.Employees.Events;

/// <summary>
/// Domain event raised when a new employee is created.
/// </summary>
public record EmployeeCreatedEvent(Employee Employee) : IDomainEvent;
