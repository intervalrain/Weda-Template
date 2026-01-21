namespace Weda.Core.Infrastructure.Messaging;

public class WedaMessagingOptions
{
    public bool Enabled { get; set; } = true;

    public string DefaultConnection { get; set; } = "bus";

    public List<NatsConnectionConfig> Connections { get; } = [];
}