using Weda.Core.Infrastructure.Messaging.Nats.Configuration;

namespace Weda.Core.Infrastructure.Messaging.Nats;

/// <summary>
/// Factory implementation for creating IJetStreamClient instances.
/// </summary>
public class JetStreamClientFactory(INatsConnectionProvider provider) : IJetStreamClientFactory
{
    public IJetStreamClient Create(string? connection = null)
    {
        var natsConnection = provider.GetConnection(connection);
        var jetStream = provider.GetJetStreamContext(connection);
        return new JetStreamClient(natsConnection, jetStream);
    }
}