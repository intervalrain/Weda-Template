using NATS.Client.JetStream.Models;

namespace Weda.Core.Infrastructure.Messaging.Nats;

public interface IJetStreamClient
{
    Task PublishAsync<T>(string subject, T data, CancellationToken cancellationToken = default);

    Task<TReply> RequestAsync<TRequset, TReply>(string subject, TRequset data, CancellationToken cancellationToken = default);

    Task<PubAckResponse> JsPublishAsync<T>(string subject, T data, CancellationToken cancellationToken = default);
}