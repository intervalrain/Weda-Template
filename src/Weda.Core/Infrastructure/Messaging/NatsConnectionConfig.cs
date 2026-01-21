namespace Weda.Core.Infrastructure.Messaging;

public class NatsConnectionConfig
{
    public required string Name { get; set; }
    public required string Url { get; set; }
    public string? CredFile { get; set; }
}