namespace Weda.Core.Infrastructure.Outbox;

public class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int MaxRetries { get; set; } = 5;
    public int BatchSize { get; set; } = 100;
    public TimeSpan ProcessingInterval { get; set; } = TimeSpan.FromSeconds(5);
    public string? ConnectionName { get; set; }

    public double FailureRatio { get; set; } = 0.5;
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(7);
    public bool DeleteProcessedMessages = true;
}