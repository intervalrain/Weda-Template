---
title: 基礎設施模式指南
description: Unit of Work、Distributed Cache、Outbox Pattern 的實作指南
keywords: [Unit of Work, Distributed Cache, Outbox Pattern, NATS, Resilience]
sidebar_position: 13
---

# 基礎設施模式指南

> 學習 Weda.Core 提供的基礎設施模式：Unit of Work、分散式快取、Outbox Pattern

## 概觀

Weda.Core 提供以下基礎設施模式：

| 模式 | 用途 | 技術 |
|------|------|------|
| Unit of Work | 交易邊界管理 | EF Core + Pipeline Behavior |
| Distributed Cache | 分散式快取 | NATS KV Store |
| Outbox Pattern | 可靠訊息發布 | NATS JetStream + Polly |

```
src/Weda.Core/
├── Application/
│   ├── Behaviors/
│   │   └── UnitOfWorkBehavior.cs     # 自動 SaveChanges
│   └── Interfaces/
│       └── IUnitOfWork.cs            # 交易邊界介面
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

### 1.1 概念

Unit of Work 確保一個業務操作中的所有變更在同一個交易中完成：

```
Command Handler
    ↓
Repository.AddAsync()     ← 只標記變更
Repository.UpdateAsync()  ← 只標記變更
    ↓
UnitOfWorkBehavior
    ↓
SaveChangesAsync()        ← 一次性提交所有變更
```

### 1.2 IUnitOfWork 介面

```csharp
namespace Weda.Core.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

`WedaDbContext` 實作此介面：

```csharp
public abstract class WedaDbContext : DbContext, IUnitOfWork
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 收集 Domain Events
        var domainEvents = ChangeTracker.Entries<IAggregateRoot>()
           .SelectMany(entry => entry.Entity.PopDomainEvents())
           .ToList();

        // 儲存變更
        var result = await base.SaveChangesAsync(cancellationToken);

        // 發布 Domain Events
        await PublishDomainEvents(domainEvents);

        return result;
    }
}
```

### 1.3 UnitOfWorkBehavior

Pipeline Behavior 在 Command 完成後自動呼叫 `SaveChangesAsync()`：

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

        // 只對 Command 執行 SaveChanges
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

### 1.4 Pipeline 執行順序

```
Request
    ↓
AuthorizationBehavior  ← 檢查權限
    ↓
ValidationBehavior     ← 驗證輸入
    ↓
Handler                ← 業務邏輯
    ↓
UnitOfWorkBehavior     ← SaveChanges (僅 Command)
    ↓
Response
```

### 1.5 Repository 實作

Repository 只標記變更，不直接儲存：

```csharp
public class GenericRepository<T, TId, TDbContext> : IRepository<T, TId>
{
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        // 不呼叫 SaveChangesAsync()
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
        // 不呼叫 SaveChangesAsync()
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
        // 不呼叫 SaveChangesAsync()
    }
}
```

---

## 2. Distributed Cache (NATS KV)

### 2.1 概念

使用 NATS KV Store 實作 `IDistributedCache`，提供分散式快取能力：

```
Application
    ↓
IDistributedCache (標準 .NET 介面)
    ↓
NatsKvDistributedCache (實作)
    ↓
NATS KV Store
```

### 2.2 啟用快取

在 `Program.cs` 中設定：

```csharp
services.AddNats(builder =>
{
    builder.BindConfiguration(natsOptions);
    builder.AddKeyValueCache(opts =>
    {
        opts.BucketName = "app-cache";      // KV bucket 名稱
        opts.DefaultTtl = TimeSpan.FromMinutes(30);  // 預設 TTL
        opts.ConnectionName = null;          // 使用 default connection
    });
});
```

### 2.3 使用快取

注入 `IDistributedCache` 使用：

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

        // 嘗試從快取取得
        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<EmployeeDto>(cached)!;
        }

        // 從資料庫取得
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null)
        {
            return EmployeeErrors.NotFound(request.Id);
        }

        var dto = EmployeeMapper.ToDto(employee);

        // 寫入快取
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

| 屬性 | 預設值 | 說明 |
|------|--------|------|
| `BucketName` | `"cache"` | NATS KV bucket 名稱 |
| `ConnectionName` | `null` | NATS connection 名稱（null = default） |
| `DefaultTtl` | `1 hour` | 預設過期時間 |

---

## 3. Outbox Pattern

### 3.1 概念

Outbox Pattern 確保訊息可靠發布到 NATS JetStream：

```
Transaction
    ↓
1. 儲存業務資料
2. 寫入 OutboxMessage 表
    ↓
Commit
    ↓
OutboxProcessor (背景服務)
    ↓
發布到 NATS JetStream
    ↓
標記為已處理
```

### 3.2 啟用 Outbox

在 `Program.cs` 中設定：

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
    public string Payload { get; private set; }      // JSON 序列化內容
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

### 3.4 寫入 Outbox

在 Domain Event Handler 中寫入 Outbox：

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
        // SaveChanges 由 UnitOfWork 處理
    }
}
```

### 3.5 Exponential Backoff

失敗重試採用指數退避策略：

| 重試次數 | 延遲時間 |
|----------|----------|
| 1 | 2 秒 |
| 2 | 4 秒 |
| 3 | 8 秒 |
| 4 | 16 秒 |
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

使用 `Microsoft.Extensions.Resilience` 實作 Circuit Breaker：

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

Circuit Breaker 狀態：

```
Closed → (失敗率超過 50%) → Open → (30秒後) → Half-Open → (成功) → Closed
                                                    ↓
                                               (失敗) → Open
```

### 3.7 OutboxOptions

| 屬性 | 預設值 | 說明 |
|------|--------|------|
| `MaxRetries` | `5` | 最大重試次數 |
| `BatchSize` | `100` | 每次處理的訊息數量 |
| `ProcessingInterval` | `5s` | 處理間隔 |
| `RetentionPeriod` | `7 days` | 已處理訊息保留時間 |
| `DeleteProcessedMessages` | `true` | 是否自動刪除已處理訊息 |
| `FailureRatio` | `0.5` | Circuit Breaker 失敗率閾值 |
| `SamplingDuration` | `30s` | Circuit Breaker 取樣時間 |
| `BreakDuration` | `30s` | Circuit Breaker 斷開時間 |

### 3.8 DbContext 設定

在 `AppDbContext` 的 `OnModelCreating` 中設定 `OutboxMessage`：

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.ConfigureOutboxMessage();
}
```

或使用 extension method：

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

## 4. 完整設定範例

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
            // 分散式快取
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

## 快速參考

### Pattern 選擇

| 需求 | Pattern |
|------|---------|
| 確保一個操作中的所有變更一起提交 | Unit of Work |
| 減少資料庫查詢，提升效能 | Distributed Cache |
| 確保訊息可靠發布到外部系統 | Outbox Pattern |

### 錯誤處理流程

```
Outbox Message
    ↓
發布失敗 → Exponential Backoff (2s, 4s, 8s, 16s, 32s)
    ↓
超過 MaxRetries → Dead Letter Queue
    ↓
Circuit Breaker Open → 暫停處理 30s
```

---

## 相關資源

- [GUIDE.md](GUIDE.md) - 學習指南總覽
- [09-error-handling-guide.md](09-error-handling-guide.md) - 錯誤處理（ErrorOr Pattern）
- [11-domain-events-guide.md](11-domain-events-guide.md) - Domain Events
- [Microsoft.Extensions.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/)
- [NATS KV Store](https://docs.nats.io/nats-concepts/jetstream/key-value-store)