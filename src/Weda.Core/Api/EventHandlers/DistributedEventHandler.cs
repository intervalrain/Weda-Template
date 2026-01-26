// DEPRECATED: This class is from EdgeSync.ServiceFramework and has been replaced by EventController.
// Please use Weda.Core.Infrastructure.Nats.EventController instead.
// This file is kept for backward compatibility and will be removed in a future version.
//
// Migration Guide:
// 1. Change base class from DistributedEventHandler<T> to EventController
// 2. Add [Subject] attribute to specify NATS subject
// 3. Use [Stream] and [Consumer] attributes for JetStream configuration
// 4. Remove IJetStreamClientFactory dependency
// 5. Use Mediator, Logger, and NatsProvider from base class
//
// Example:
// [Stream("orders_stream")]
// [Consumer("order_handler")]
// public class OrderEventController : EventController
// {
//     [Subject("order.created")]
//     public async Task OnOrderCreated(OrderCreatedEvent evt)
//     {
//         // Handle event
//     }
// }

#if FALSE // Disabled - use EventController instead

using System.Text;
using System.Text.Json;

using EdgeSync.ServiceFramework.Abstractions.JetStream;
using EdgeSync.ServiceFramework.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Weda.Core.Api.EventHandlers;

public abstract class DistributedEventHandler<T>(
    ILogger<BaseEventHandler> logger,
    IJetStreamClientFactory factory,
    IServiceScopeFactory scopeFactory,
    string connectionName = "bus") : BaseEventHandler(logger, factory, connectionName)
{
    protected abstract Task HandleAsync(T @event, string subject, IServiceProvider serviceProvider);

    protected override async Task HandleInputEventCore(byte[] message, string subject)
    {
        var json = Encoding.UTF8.GetString(message);
        var data = JsonSerializer.Deserialize<T>(json);

        if (data is null)
        {
            throw new JsonException("Cannot deserialize message.");
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        await HandleAsync(data, subject, scope.ServiceProvider);
    }
}

#endif