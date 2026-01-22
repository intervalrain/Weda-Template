using EdgeSync.ServiceFramework.Abstractions.JetStream;
using EdgeSync.ServiceFramework.Core;

using Mediator;

using Weda.Core.Api.EventHandlers;
using Weda.Template.Contracts.Employees.Commands;

namespace Weda.Template.Api.EventHandlers.Employees;

public class CreateEmployeeEventHandler(
    ILogger<BaseEventHandler> logger,
    IJetStreamClientFactory factory,
    IServiceScopeFactory scopeFactory,
    string connectionName = "bus") : DistributedEventHandler<CreateEmployeeCommand>(logger, factory, scopeFactory, connectionName)
{
    protected override string SubjectName => "eco1j.weda.*.*.emp.create";
    protected override string StreamName => "create_emp_stream";
    protected override string ConsumerName => "create_emp_consumer";

    protected override async Task HandleAsync(CreateEmployeeCommand @event, string subject, IServiceProvider serviceProvider)
    {
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        await mediator.Send(@event, default);
    }
}