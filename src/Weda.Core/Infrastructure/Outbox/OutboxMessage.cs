namespace Weda.Core.Infrastructure.Outbox;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public string? Error { get; set; }
    public int RetryCount { get; private set; }
    public OutboxMessageStatus Status { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string type, string payload)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Payload = payload,
            CreatedAt = DateTime.UtcNow,
            Status = OutboxMessageStatus.Pending
        };
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Status = OutboxMessageStatus.Processed;
        Error = null;
        NextRetryAt = null;
    }

    public void MarkAsFailed(string error, int maxRetries)
    {
        Error = error;
        RetryCount++;

        if (RetryCount >= maxRetries)
        {
            Status = OutboxMessageStatus.DeadLettered;
            NextRetryAt = null;
        }
        else
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, RetryCount));
            NextRetryAt = DateTime.UtcNow.Add(delay);
        }
    }
}