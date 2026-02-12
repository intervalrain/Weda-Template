---
title: Infrastructure Patterns Guide
description: Implementation guide for Unit of Work, Distributed Cache, and Outbox Pattern
keywords: [Unit of Work, Distributed Cache, Outbox Pattern, NATS, Resilience]
sidebar_position: 13
---

# Infrastructure Patterns Guide

> Learn about infrastructure patterns provided by Weda.Core: Unit of Work, Distributed Cache, Outbox Pattern

## Overview

Weda.Core provides the following infrastructure patterns:

| Pattern | Purpose | Technology |
|---------|---------|------------|
| Unit of Work | Transaction boundary management | EF Core + Pipeline Behavior |
| Distributed Cache | Distributed caching | NATS KV Store |
| Outbox Pattern | Reliable message publishing | NATS JetStream + Polly |

```
src/Weda.Core/
├── Application/
│   ├── Behaviors/
│   │   └── UnitOfWorkBehavior.cs     # Auto SaveChanges
│   └── Interfaces/
│       └── IUnitOfWork.cs            # Transaction boundary interface
├── Infrastructure/
│   ├── Messaging/Nats/
│   │   ├── Caching/
│   │   │   ├── NatsKvDistributedCache.cs
│   │   │   └── NatsKvCacheOptions.cs
│   │   └── Configuration/
│   │       └── NatsBuilder.cs        # Fluent API
│   └── Outbox/
│       ├── OutboxMessage.cs
│       ├── OutboxProcessor.cs
│       └── OutboxOptions.cs
```

---

## 1. Unit of Work Pattern

### 1.1 Concept

Unit of Work ensures all changes within a business operation complete in the same transaction:

```
Command Handler
    ↓
Repository.AddAsync()     ← Only marks changes
Repository.UpdateAsync()  ← Only marks changes
    ↓
UnitOfWorkBehavior
    ↓
SaveChangesAsync()        ← Commits all changes at once
```

### 1.2 IUnitOfWork Interface

```csharp
namespace Weda.Core.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

`WedaDbContext` implements this interface:

```csharp
public abstract class WedaDbContext : DbContext, IUnitOfWork
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect Domain Events
        var domainEvents = ChangeTracker.Entries<IAggregateRoot>()
           .SelectMany(entry => entry.Entity.PopDomainEvents())
           .ToList();

        // Save changes
        var result = await base.SaveChangesAsync(cancellationToken);

        // Publish Domain Events
        await PublishDomainEvents(domainEvents);

        return result;
    }
}
```

### 1.3 UnitOfWorkBehavior

Pipeline Behavior automatically calls `SaveChangesAsync()` after Command completion:

```csharp
public class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(message, cancellationToken);

        // Only execute SaveChanges for Commands
        if (IsCommand())
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }

    private static bool IsCommand()
    {
        return typeof(TRequest).GetInterfaces()
            .Any(i => i.IsGenericType &&
                      i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }
}
```

### 1.4 Pipeline Execution Order

```
Request
    ↓
AuthorizationBehavior  ← Check permissions
    ↓
ValidationBehavior     ← Validate input
    ↓
Handler                ← Business logic
    ↓
UnitOfWorkBehavior     ← SaveChanges (Commands only)
    ↓
Response
```

### 1.5 Repository Implementation

Repository only marks changes, does not save directly:

```csharp
public class GenericRepository<T, TId, TDbContext> : IRepository<T, TId>
{
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        // Does not call SaveChangesAsync()
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
        // Does not call SaveChangesAsync()
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
        // Does not call SaveChangesAsync()
    }
}
```

---

## 2. Distributed Cache (NATS KV)

### 2.1 Concept

Uses NATS KV Store to implement `IDistributedCache`, providing distributed caching capability:

```
Application
    ↓
IDistributedCache (Standard .NET interface)
    ↓
NatsKvDistributedCache (Implementation)
    ↓
NATS KV Store
```

### 2.2 Enable Cache

Configure in `Program.cs`:

```csharp
services.AddNats(builder =>
{
    builder.BindConfiguration(natsOptions);
    builder.AddKeyValueCache(opts =>
    {
        opts.BucketName = "app-cache";      // KV bucket name
        opts.DefaultTtl = TimeSpan.FromMinutes(30);  // Default TTL
        opts.ConnectionName = null;          // Use default connection
    });
});
```

### 2.3 Using Cache

Inject `IDistributedCache` to use:

```csharp
public class GetEmployeeQueryHandler(
    IEmployeeRepository repository,
    IDistributedCache cache)
{
    public async ValueTask<ErrorOr<EmployeeDto>> Handle(
        GetEmployeeQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"employee:{request.Id}";

        // Try to get from cache
        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<EmployeeDto>(cached)!;
        }

        // Get from database
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null)
        {
            return EmployeeErrors.NotFound(request.Id);
        }

        var dto = EmployeeMapper.ToDto(employee);

        // Write to cache
        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(dto),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            },
            cancellationToken);

        return dto;
    }
}
```

### 2.4 NatsKvCacheOptions

| Property | Default | Description |
|----------|---------|-------------|
| `BucketName` | `"cache"` | NATS KV bucket name |
| `ConnectionName` | `null` | NATS connection name (null = default) |
| `DefaultTtl` | `1 hour` | Default expiration time |

---

## 3. Outbox Pattern

### 3.1 Concept

Outbox Pattern ensures reliable message publishing to NATS JetStream:

```
Transaction
    ↓
1. Save business data
2. Write to OutboxMessage table
    ↓
Commit
    ↓
OutboxProcessor (Background service)
    ↓
Publish to NATS JetStream
    ↓
Mark as processed
```

### 3.2 Enable Outbox

Configure in `Program.cs`:

```csharp
services.AddNats(builder =>
{
    builder.BindConfiguration(natsOptions);
    builder.AddOutbox<AppDbContext>(opts =>
    {
        opts.MaxRetries = 5;
        opts.BatchSize = 100;
        opts.ProcessingInterval = TimeSpan.FromSeconds(5);
        opts.RetentionPeriod = TimeSpan.FromDays(7);
    });
});
```

### 3.3 OutboxMessage Entity

```csharp
public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; }         // Subject/Topic
    public string Payload { get; private set; }      // JSON serialized content
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
    public OutboxMessageStatus Status { get; private set; }

    public static OutboxMessage Create(string type, string payload);
    public void MarkAsProcessed();
    public void MarkAsFailed(string error, int maxRetries);
}

public enum OutboxMessageStatus
{
    Pending = 0,
    Processed = 1,
    DeadLettered = 2
}
```

### 3.4 Writing to Outbox

Write to Outbox in Domain Event Handler:

```csharp
public class EmployeeCreatedEventHandler(AppDbContext dbContext)
    : INotificationHandler<EmployeeCreatedEvent>
{
    public async ValueTask Handle(
        EmployeeCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new EmployeeCreatedIntegrationEvent
        {
            EmployeeId = notification.Employee.Id,
            Name = notification.Employee.Name,
            Email = notification.Employee.Email.Value
        };

        var outboxMessage = OutboxMessage.Create(
            type: "employee.created",
            payload: JsonSerializer.Serialize(integrationEvent));

        dbContext.Set<OutboxMessage>().Add(outboxMessage);
        // SaveChanges handled by UnitOfWork
    }
}
```

### 3.5 Exponential Backoff

Failed retries use exponential backoff strategy:

| Retry Count | Delay |
|-------------|-------|
| 1 | 2 seconds |
| 2 | 4 seconds |
| 3 | 8 seconds |
| 4 | 16 seconds |
| 5 | Dead Letter |

```csharp
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
        // Exponential backoff: 2^retryCount seconds
        var delay = TimeSpan.FromSeconds(Math.Pow(2, RetryCount));
        NextRetryAt = DateTime.UtcNow.Add(delay);
    }
}
```

### 3.6 Circuit Breaker

Uses `Microsoft.Extensions.Resilience` to implement Circuit Breaker:

```csharp
private static ResiliencePipeline CreateResiliencePipeline(OutboxOptions options)
{
    return new ResiliencePipelineBuilder()
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = options.FailureRatio,      // 50%
            SamplingDuration = options.SamplingDuration, // 30s
            BreakDuration = options.BreakDuration        // 30s
        })
        .Build();
}
```

Circuit Breaker states:

```
Closed → (Failure rate exceeds 50%) → Open → (After 30s) → Half-Open → (Success) → Closed
                                                    ↓
                                               (Failure) → Open
```

### 3.7 OutboxOptions

| Property | Default | Description |
|----------|---------|-------------|
| `MaxRetries` | `5` | Maximum retry count |
| `BatchSize` | `100` | Messages processed per batch |
| `ProcessingInterval` | `5s` | Processing interval |
| `RetentionPeriod` | `7 days` | Processed message retention period |
| `DeleteProcessedMessages` | `true` | Auto-delete processed messages |
| `FailureRatio` | `0.5` | Circuit Breaker failure ratio threshold |
| `SamplingDuration` | `30s` | Circuit Breaker sampling duration |
| `BreakDuration` | `30s` | Circuit Breaker break duration |

### 3.8 DbContext Configuration

Configure `OutboxMessage` in `AppDbContext`'s `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.ConfigureOutboxMessage();
}
```

Or use extension method:

```csharp
public static class OutboxMessageModelBuilderConfiguration
{
    public static ModelBuilder ConfigureOutboxMessage(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Payload).IsRequired();
            builder.Property(x => x.Status).HasConversion<int>();
            builder.HasIndex(x => new { x.Status, x.NextRetryAt });
        });

        return modelBuilder;
    }
}
```

---

## 4. Complete Configuration Example

### 4.1 Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWedaCore<IAssemblyMarker, IContractsMarker, IApplicationMarker>(
        builder.Configuration,
        services => services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [typeof(IApplicationMarker).Assembly];
        }),
        options =>
        {
            options.OpenApiInfo = new OpenApiInfo
            {
                Title = "Weda API",
                Version = "v1",
            };
        },
        nats =>
        {
            // Distributed Cache
            nats.AddKeyValueCache(opts =>
            {
                opts.BucketName = "app-cache";
                opts.DefaultTtl = TimeSpan.FromMinutes(30);
            });

            // Outbox Pattern
            nats.AddOutbox<AppDbContext>(opts =>
            {
                opts.MaxRetries = 5;
                opts.RetentionPeriod = TimeSpan.FromDays(7);
            });
        }
    );
```

### 4.2 appsettings.json

```json
{
  "Nats": {
    "DefaultConnection": "default",
    "Connections": {
      "default": {
        "Url": "nats://localhost:4222"
      }
    }
  },
  "Outbox": {
    "MaxRetries": 5,
    "BatchSize": 100,
    "ProcessingInterval": "00:00:05",
    "RetentionPeriod": "7.00:00:00"
  }
}
```

---

## Quick Reference

### Pattern Selection

| Requirement | Pattern |
|-------------|---------|
| Ensure all changes in an operation commit together | Unit of Work |
| Reduce database queries, improve performance | Distributed Cache |
| Ensure reliable message publishing to external systems | Outbox Pattern |

### Error Handling Flow

```
Outbox Message
    ↓
Publish failed → Exponential Backoff (2s, 4s, 8s, 16s, 32s)
    ↓
Exceeds MaxRetries → Dead Letter Queue
    ↓
Circuit Breaker Open → Pause processing 30s
```

---

## Related Resources

- [GUIDE.md](GUIDE.md) - Learning Guide Overview
- [09-error-handling-guide.md](09-error-handling-guide.md) - Error Handling (ErrorOr Pattern)
- [11-domain-events-guide.md](11-domain-events-guide.md) - Domain Events
- [Microsoft.Extensions.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/)
- [NATS KV Store](https://docs.nats.io/nats-concepts/jetstream/key-value-store)