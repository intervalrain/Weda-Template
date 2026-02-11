namespace Weda.Core.Infrastructure.Outbox;

public enum OutboxMessageStatus
{
    Pending = 0,
    Processed = 1,
    DeadLettered = 2,
}