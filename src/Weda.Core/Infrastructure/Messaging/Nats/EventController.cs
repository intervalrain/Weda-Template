using Mediator;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using Weda.Core.Infrastructure.Messaging.Nats.Attributes;
using Weda.Core.Infrastructure.Messaging.Nats.Configuration;

namespace Weda.Core.Infrastructure.Messaging.Nats;

[Stream("employees_v{version:apiVersion}_stream")]
[Consumer("employees_v{version:apiVersion}_consumer")]
[Connection("bus")]
public abstract class EventController
{
    public IMediator Mediator { get; internal set; } = null!;
    public INatsConnectionProvider NatsProvider { get; internal set; } = null!;
    public ILogger Logger { get; internal set; } = null!;
    public string Subject { get; internal set; } = string.Empty;
    public NatsHeaders? Headers { get; internal set; }

    /// <summary>
    /// Values extracted from subject placeholders (e.g., {id} from "employee.v1.123.get").
    /// </summary>
    public IReadOnlyDictionary<string, string> SubjectValues { get; internal set; } = new Dictionary<string, string>();

    protected INatsConnection GetConnection(string? name = null) => NatsProvider.GetConnection(name);
    protected NatsJSContext GetJetStream(string? name = null) => NatsProvider.GetJetStreamContext(name);
}