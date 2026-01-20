using Weda.Core.Domain;
using Weda.Template.Domain.Employees.Entities;

namespace Weda.Template.Domain.Employees.Events;

/// <summary>
/// Domain event raised when a supervisor is assigned to an employee.
/// </summary>
public record SupervisorAssignedEvent(Employee Employee, int? SupervisorId) : IDomainEvent;
