using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

using Weda.Core.Infrastructure.Nats.Configuration;
using Weda.Core.Infrastructure.Nats.Discovery;

namespace Weda.Core.Infrastructure.Nats.Hosting;

public class JetStreamFetchHostedService(
    EventControllerDiscovery discovery,
    INatsConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    ILogger<JetStreamFetchHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var endpoints = discovery.JetStreamFetchEndpoints;

        if (endpoints.Count == 0)
        {
            logger.LogInformation("No JetStream Fetch endpoints found");
            return;
        }

        logger.LogInformation("Starting JetStream Fetch (FetchAsync) consumers...");

        var tasks = new List<Task>();

        foreach (var endpoint in endpoints)
        {
            var task = FetchAsync(endpoint, stoppingToken);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    private async Task FetchAsync(EndpointDescriptor endpoint, CancellationToken stoppingToken)
    {
        var js = connectionProvider.GetJetStreamContext(endpoint.ConnectionName);

        // Ensure stream exists
        if (endpoint.StreamName is null)
        {
            logger.LogWarning(
                "JetStream Fetch endpoint {Controller}.{Method} has no StreamName",
                endpoint.ControllerType.Name,
                endpoint.Method.Name);
            return;
        }

        // Ensure consumer exists
        if (endpoint.ConsumerName is null)
        {
            logger.LogWarning(
                "JetStream Fetch endpoint {Controller}.{Method} has no ConsumerName",
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
                "JetStream Fetch: {Subject} (Stream: {Stream}, Consumer: {Consumer}) -> {Controller}.{Method}",
                subject,
                endpoint.StreamName,
                endpoint.ConsumerName,
                endpoint.ControllerType.Name,
                endpoint.Method.Name);

            // Fetch mode: continuously fetch batches
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await foreach (var msg in consumer.FetchAsync<byte[]>(
                        opts: new NatsJSFetchOpts { MaxMsgs = 10, Expires = TimeSpan.FromSeconds(5) },
                        cancellationToken: stoppingToken))
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

                                    logger.LogDebug("JetStream Fetch processed: {Subject}", msg.Subject);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError(ex, "Error processing JetStream Fetch: {Subject}", msg.Subject);

                                    // Ack the message even on error to prevent infinite retry loops.
                                    // Business logic errors (e.g., duplicate email) should not cause redelivery.
                                    // For transient errors, consider implementing a dead-letter queue pattern.
                                    await msg.AckAsync(cancellationToken: stoppingToken);
                                }
                            },
                            stoppingToken);
                    }

                    // Small delay between fetch batches if no messages
                    await Task.Delay(100, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error fetching messages from {Consumer}", endpoint.ConsumerName);
                    await Task.Delay(1000, stoppingToken); // Backoff on error
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to setup JetStream Fetch consumer {Consumer} on stream {Stream}",
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

            var streamConfig = new StreamConfig
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
            await js.CreateOrUpdateConsumerAsync(streamName, new ConsumerConfig(consumerName), cancellationToken);
            logger.LogDebug("Consumer {Consumer} on stream {Stream} already exists", consumerName, streamName);
        }
        catch (NatsJSApiException ex) when (ex.Error.Code == 404)
        {
            // Consumer doesn't exist, create it
            logger.LogInformation("Creating consumer {Consumer} on stream {Stream}", consumerName, streamName);

            var consumerConfig = new ConsumerConfig
            {
                Name = consumerName,
                DurableName = consumerName,
                AckPolicy = ConsumerConfigAckPolicy.Explicit,
            };

            await js.CreateOrUpdateConsumerAsync(streamName, consumerConfig, cancellationToken);
            logger.LogInformation("Consumer {Consumer} created successfully", consumerName);
        }
    }
}