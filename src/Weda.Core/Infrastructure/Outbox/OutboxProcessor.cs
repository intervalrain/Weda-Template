using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.CircuitBreaker;

using Weda.Core.Infrastructure.Messaging.Nats.Configuration;

namespace Weda.Core.Infrastructure.Outbox;

public class OutboxProcessor<TDbContext>(
    IServiceScopeFactory scopeFactory,
    INatsConnectionProvider connectionProvider,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext
{
    private readonly OutboxOptions _options = options.Value;
    private readonly ResiliencePipeline _pipeline = CreateResiliencePipeline(options.Value);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessageAsync(stoppingToken);
            }
            catch (BrokenCircuitException)
            {
                logger.LogWarning("Circuit breaker is open, skipping processing");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_options.ProcessingInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessageAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .Where(m => m.NextRetryAt == null || m.NextRetryAt <= DateTime.UtcNow)
            .OrderBy(m => m.CreatedAt)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) return;

        var js = connectionProvider.GetJetStreamContext(_options.ConnectionName);

        foreach (var message in messages)
        {
            try
            {
                await _pipeline.ExecuteAsync(async token =>
                {
                    await js.PublishAsync(message.Type, message.Payload, cancellationToken: token);
                }, cancellationToken);

                message.MarkAsProcessed();
                logger.LogDebug("Published outbox message {Id} to {Subject}", message.Id, message.Type);
            }
            catch (Exception ex)
            {
                message.MarkAsFailed(ex.Message, _options.MaxRetries);
                logger.LogWarning(ex, "Failed to publish outbox message {Id}, retry {Retry}",
                    message.Id, message.RetryCount);
            }
        }

        if (_options.DeleteProcessedMessages)
        {
            var cutoff = DateTime.UtcNow.Subtract(_options.RetentionPeriod);
            await dbContext.Set<OutboxMessage>()
                .Where(m => m.Status == OutboxMessageStatus.Processed)
                .Where(m => m.ProcessedAt < cutoff)
                .ExecuteDeleteAsync(cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ResiliencePipeline CreateResiliencePipeline(OutboxOptions options)
    {
        return new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = options.FailureRatio,
                SamplingDuration = options.SamplingDuration,
                BreakDuration = options.BreakDuration
            })
            .Build();
    }
}