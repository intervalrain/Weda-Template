using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NATS.Client.JetStream;

using Weda.Core.Infrastructure.Nats.Configuration;
using Weda.Core.Infrastructure.Nats.Discovery;

namespace Weda.Core.Infrastructure.Nats.Hosting;

public class JetStreamConsumeHostedService(
    EventControllerDiscovery discovery,
    INatsConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    ILogger<JetStreamConsumeHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var endpoints = discovery.JetStreamConsumeEndpoints;

        if (endpoints.Count == 0)
        {
            logger.LogInformation("No JetStream Consume endpoints found");
            return;
        }

        logger.LogInformation("Starting JetStream Consume (ConsumeAsync) subscriptions...");

        var tasks = new List<Task>();

        foreach (var endpoint in endpoints)
        {
            var task = SubscribeAsync(endpoint, stoppingToken);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    private async Task SubscribeAsync(EndpointDescriptor endpoint, CancellationToken stoppingToken)
    {
        var js = connectionProvider.GetJetStreamContext(endpoint.ConnectionName);

        // Ensure stream exists
        if (endpoint.StreamName is null)
        {
            logger.LogWarning(
                "JetStream Consume endpoint {Controller}.{Method} has no StreamName",
                endpoint.ControllerType.Name,
                endpoint.Method.Name);
            return;
        }

        // Ensure consumer exists
        if (endpoint.ConsumerName is null)
        {
            logger.LogWarning(
                "JetStream Consume endpoint {Controller}.{Method} has no ConsumerName",
                endpoint.ControllerType.Name,
                endpoint.Method.Name);
            return;
        }

        try
        {
            var subject = TemplateResolver.Resolve(endpoint.SubjectPattern, endpoint.ControllerType);

            // Ensure stream exists or create it
            await EnsureStreamExistsAsync(js, endpoint.StreamName, subject, stoppingToken);

            // Ensure consumer exists or create it
            await EnsureConsumerExistsAsync(js, endpoint.StreamName, endpoint.ConsumerName, stoppingToken);

            var consumer = await js.GetConsumerAsync(endpoint.StreamName, endpoint.ConsumerName, stoppingToken);

            logger.LogInformation(
                "JetStream Consume: {Subject} (Stream: {Stream}, Consumer: {Consumer}) -> {Controller}.{Method}",
                subject,
                endpoint.StreamName,
                endpoint.ConsumerName,
                endpoint.ControllerType.Name,
                endpoint.Method.Name);

            await foreach (var msg in consumer.ConsumeAsync<byte[]>(cancellationToken: stoppingToken))
            {
                _ = Task.Run(
                    async () =>
                    {
                        await using var scope = scopeFactory.CreateAsyncScope();
                        var invoker = scope.ServiceProvider.GetRequiredService<EventControllerInvoker>();

                        try
                        {
                            await invoker.InvokeAsync(
                                endpoint,
                                msg.Data,
                                msg.Subject,
                                msg.Headers,
                                stoppingToken);

                            await msg.AckAsync(cancellationToken: stoppingToken);

                            logger.LogDebug("JetStream Consume processed: {Subject}", msg.Subject);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error processing JetStream Consume: {Subject}", msg.Subject);

                            // Ack the message even on error to prevent infinite retry loops.
                            // Business logic errors (e.g., duplicate email) should not cause redelivery.
                            // For transient errors, consider implementing a dead-letter queue pattern.
                            await msg.AckAsync(cancellationToken: stoppingToken);
                        }
                    },
                    stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to setup JetStream Consume consumer {Consumer} on stream {Stream}",
                endpoint.ConsumerName,
                endpoint.StreamName);
        }
    }

    private async Task EnsureStreamExistsAsync(
        NatsJSContext js,
        string streamName,
        string subject,
        CancellationToken cancellationToken)
    {
        try
        {
            var stream = await js.GetStreamAsync(streamName, cancellationToken: cancellationToken);

            // Check if subject is already in the stream, if not add it
            var currentSubjects = stream.Info.Config.Subjects ?? [];
            if (!currentSubjects.Contains(subject))
            {
                var updatedSubjects = currentSubjects.Append(subject).ToList();
                var updatedConfig = stream.Info.Config with { Subjects = updatedSubjects };
                await js.UpdateStreamAsync(updatedConfig, cancellationToken);
                logger.LogInformation("Added subject {Subject} to stream {Stream}", subject, streamName);
            }
            else
            {
                logger.LogDebug("Stream {Stream} already has subject {Subject}", streamName, subject);
            }
        }
        catch (NatsJSApiException ex) when (ex.Error.Code == 404)
        {
            // Stream doesn't exist, create it
            logger.LogInformation("Creating stream {Stream} with subject {Subject}", streamName, subject);

            var streamConfig = new NATS.Client.JetStream.Models.StreamConfig
            {
                Name = streamName,
                Subjects = [subject],
            };

            await js.CreateStreamAsync(streamConfig, cancellationToken);
            logger.LogInformation("Stream {Stream} created successfully", streamName);
        }
    }

    private async Task EnsureConsumerExistsAsync(
        NatsJSContext js,
        string streamName,
        string consumerName,
        CancellationToken cancellationToken)
    {
        try
        {
            await js.GetConsumerAsync(streamName, consumerName, cancellationToken);
            logger.LogDebug("Consumer {Consumer} on stream {Stream} already exists", consumerName, streamName);
        }
        catch (NatsJSApiException ex) when (ex.Error.Code == 404)
        {
            // Consumer doesn't exist, create it
            logger.LogInformation("Creating consumer {Consumer} on stream {Stream}", consumerName, streamName);

            var consumerConfig = new NATS.Client.JetStream.Models.ConsumerConfig
            {
                Name = consumerName,
                DurableName = consumerName,
                AckPolicy = NATS.Client.JetStream.Models.ConsumerConfigAckPolicy.Explicit,
            };

            await js.CreateOrUpdateConsumerAsync(streamName, consumerConfig, cancellationToken);
            logger.LogInformation("Consumer {Consumer} created successfully", consumerName);
        }
    }
}
