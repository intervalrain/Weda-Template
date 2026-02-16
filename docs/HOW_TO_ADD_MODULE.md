# How to Add a New Module

This guide walks you through creating a new module (feature/aggregate) in your Weda project. We'll use "Product" as an example.

## Overview

A complete module consists of:

1. **Domain** - Entity, Value Objects, Domain Events
2. **Contracts** - DTOs, Request/Response models
3. **Application** - Commands, Queries, Handlers
4. **Infrastructure** - Repository, EF Configuration
5. **API** - Controllers, Endpoints

---

## Step 1: Domain Layer

### 1.1 Create the Entity

**File:** `src/YourProject.Domain/Products/Product.cs`

```csharp
using YourProject.Domain.Common;

namespace YourProject.Domain.Products;

public class Product : Entity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    private Product() { } // EF Core

    public Product(string name, decimal price)
    {
        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        Stock = 0;

        AddDomainEvent(new ProductCreatedEvent(Id, Name));
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new DomainException("Price cannot be negative");

        var oldPrice = Price;
        Price = newPrice;

        AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive");

        Stock += quantity;
    }
}
```

### 1.2 Create Domain Events

**File:** `src/YourProject.Domain/Products/Events/ProductCreatedEvent.cs`

```csharp
using YourProject.Domain.Common;

namespace YourProject.Domain.Products.Events;

public record ProductCreatedEvent(Guid ProductId, string Name) : IDomainEvent;

public record ProductPriceChangedEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice) : IDomainEvent;
```

### 1.3 Create Repository Interface

**File:** `src/YourProject.Domain/Products/IProductRepository.cs`

```csharp
namespace YourProject.Domain.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    void Add(Product product);
    void Update(Product product);
    void Delete(Product product);
}
```

---

## Step 2: Contracts Layer

### 2.1 Create DTOs

**File:** `src/YourProject.Contracts/Products/ProductDto.cs`

```csharp
namespace YourProject.Contracts.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock);

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price);

public record UpdateProductPriceRequest(decimal NewPrice);
```

---

## Step 3: Application Layer

### 3.1 Create Command

**File:** `src/YourProject.Application/Products/Commands/CreateProduct/CreateProductCommand.cs`

```csharp
using MediatR;
using YourProject.Domain.Common;

namespace YourProject.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price) : IRequest<Result<Guid>>;
```

### 3.2 Create Command Handler

**File:** `src/YourProject.Application/Products/Commands/CreateProduct/CreateProductCommandHandler.cs`

```csharp
using MediatR;
using YourProject.Domain.Common;
using YourProject.Domain.Products;

namespace YourProject.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = new Product(request.Name, request.Price);

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}
```

### 3.3 Create Query

**File:** `src/YourProject.Application/Products/Queries/GetProduct/GetProductQuery.cs`

```csharp
using MediatR;
using YourProject.Contracts.Products;
using YourProject.Domain.Common;

namespace YourProject.Application.Products.Queries.GetProduct;

public record GetProductQuery(Guid Id) : IRequest<Result<ProductDto>>;
```

### 3.4 Create Query Handler

**File:** `src/YourProject.Application/Products/Queries/GetProduct/GetProductQueryHandler.cs`

```csharp
using MediatR;
using YourProject.Contracts.Products;
using YourProject.Domain.Common;
using YourProject.Domain.Products;

namespace YourProject.Application.Products.Queries.GetProduct;

public class GetProductQueryHandler
    : IRequestHandler<GetProductQuery, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            request.Id, cancellationToken);

        if (product is null)
            return Result.Failure<ProductDto>(
                DomainErrors.NotFound("Product", request.Id));

        return Result.Success(new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock));
    }
}
```

---

## Step 4: Infrastructure Layer

### 4.1 Create Repository Implementation

**File:** `src/YourProject.Infrastructure/Products/ProductRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using YourProject.Domain.Products;
using YourProject.Infrastructure.Persistence;

namespace YourProject.Infrastructure.Products;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken ct = default)
    {
        return await _context.Products.ToListAsync(ct);
    }

    public void Add(Product product) => _context.Products.Add(product);

    public void Update(Product product) => _context.Products.Update(product);

    public void Delete(Product product) => _context.Products.Remove(product);
}
```

### 4.2 Create EF Configuration

**File:** `src/YourProject.Infrastructure/Products/ProductConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourProject.Domain.Products;

namespace YourProject.Infrastructure.Products;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2);
    }
}
```

### 4.3 Register in DbContext

**File:** `src/YourProject.Infrastructure/Persistence/ApplicationDbContext.cs`

Add the DbSet:

```csharp
public DbSet<Product> Products => Set<Product>();
```

### 4.4 Register Repository in DI

**File:** `src/YourProject.Infrastructure/DependencyInjection.cs`

```csharp
services.AddScoped<IProductRepository, ProductRepository>();
```

---

## Step 5: API Layer

### 5.1 Create Controller

**File:** `src/YourProject.Api/Products/ProductsController.cs`

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using YourProject.Application.Products.Commands.CreateProduct;
using YourProject.Application.Products.Queries.GetProduct;
using YourProject.Contracts.Products;

namespace YourProject.Api.Products;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _sender.Send(new GetProductQuery(id));

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price);

        var result = await _sender.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }
}
```

---

## Step 6: Create Migration

```bash
dotnet ef migrations add AddProducts \
  --project src/YourProject.Infrastructure \
  --startup-project src/YourProject.Api

dotnet ef database update \
  --project src/YourProject.Infrastructure \
  --startup-project src/YourProject.Api
```

---

## Step 7: Add Tests (Optional but Recommended)

### Unit Test Example

**File:** `tests/YourProject.Domain.UnitTests/Products/ProductTests.cs`

```csharp
using YourProject.Domain.Products;
using YourProject.Domain.Products.Events;

namespace YourProject.Domain.UnitTests.Products;

public class ProductTests
{
    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        // Arrange & Act
        var product = new Product("Test Product", 99.99m);

        // Assert
        var domainEvent = product.DomainEvents
            .OfType<ProductCreatedEvent>()
            .SingleOrDefault();

        Assert.NotNull(domainEvent);
        Assert.Equal(product.Id, domainEvent.ProductId);
    }

    [Fact]
    public void UpdatePrice_WithNegativePrice_ShouldThrow()
    {
        // Arrange
        var product = new Product("Test", 10m);

        // Act & Assert
        Assert.Throws<DomainException>(() => product.UpdatePrice(-5m));
    }
}
```

---

## Summary Checklist

- [ ] Domain Entity created
- [ ] Domain Events defined
- [ ] Repository interface defined
- [ ] DTOs created in Contracts
- [ ] Commands and Handlers created
- [ ] Queries and Handlers created
- [ ] Repository implementation created
- [ ] EF Configuration created
- [ ] DbSet added to DbContext
- [ ] Repository registered in DI
- [ ] API Controller created
- [ ] Migration created and applied
- [ ] Unit tests written

---

## Tips

1. **Start with Domain**: Always start with the domain layer - it has no dependencies
2. **Use Records**: Use C# records for DTOs, Commands, Queries, and Events
3. **Keep Handlers Small**: Each handler should do one thing
4. **Domain Events**: Raise events for significant state changes
5. **Validation**: Use FluentValidation for command validation
6. **Testing**: Write tests as you go, not after