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