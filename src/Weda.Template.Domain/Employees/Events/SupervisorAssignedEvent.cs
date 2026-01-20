using Weda.Template.Domain.Common;
using Weda.Template.Domain.Employees.Entities;

namespace Weda.Template.Domain.Employees.Events;

/// <summary>
/// Domain event raised when a supervisor is assigned to an employee.
/// </summary>
public record SupervisorAssignedEvent(Employee Employee, Guid? SupervisorId) : IDomainEvent;
