using EdgeSync.ServiceFramework.Abstractions.JetStream;

using Mediator;

using Microsoft.Extensions.Logging;

using Weda.Template.Contracts.Employees.Events;
using Weda.Template.Domain.Employees.Events;

namespace Weda.Template.Application.Employees.EventHandlers;

public class EmployeeCreatedEventHandler(
    ILogger<EmployeeCreatedEventHandler> logger,
    IJetStreamClientFactory factory)
    : INotificationHandler<EmployeeCreatedEvent>
{
    private readonly IJetStreamClient _bus = factory.CreateClient("bus");

    public async ValueTask Handle(EmployeeCreatedEvent @event, CancellationToken cancellationToken)
    {
        var employee = @event.Employee;
        var subject = EmployeeNatsSubjects.BuildCreatedEventSubject(employee.Id, "*");

        var natsEvent = new EmployeeCreatedNatsEvent(
            employee.Id,
            employee.Name.Value,
            employee.Email.Value,
            employee.Department.ToString(),
            employee.Position,
            employee.CreatedAt);

        await _bus.PublishAsync(subject, natsEvent, cancellationToken: cancellationToken);

        logger.LogInformation(
            "Published EmployeeCreatedNatsEvent for Employee {EmployeeId} to {Subject}",
            employee.Id,
            subject);
    }
}