using Mediator;
using Microsoft.Extensions.Logging;
using Weda.Template.Contracts.Employees.Events;
using Weda.Template.Domain.Employees.Events;

namespace Weda.Template.Application.Employees.EventHandlers;

public class EmployeeCreatedEventHandler(
    ILogger<EmployeeCreatedEventHandler> logger)
    : INotificationHandler<EmployeeCreatedEvent>
{
    public ValueTask Handle(EmployeeCreatedEvent @event, CancellationToken cancellationToken)
    {
        var employee = @event.Employee;
        var subject = EmployeeNatsSubjects.BuildCreatedEventSubject(employee.Id, "*");

        var natsEvent = new CreateEmployeeNatsEvent(
            employee.Name.Value,
            employee.Email.Value,
            employee.Department.ToString(),
            employee.Position);

        try
        {
            logger.LogInformation(
                "Published CreateEmployeeNatsEvent for Employee {EmployeeId} to {Subject}",
                employee.Id,
                subject);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to publish CreateEmployeeNatsEvent for Employee {EmployeeId}. NATS may not be available.",
                employee.Id);
        }

        return ValueTask.CompletedTask;
    }
}