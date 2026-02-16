# Frequently Asked Questions

## General

### What is Weda Template?

Weda Template is a .NET project template that implements Clean Architecture with DDD, CQRS, and NATS integration. It provides a solid foundation for building enterprise-grade applications.

### What .NET version is required?

.NET 10.0 or later is required. Check your version with:

```bash
dotnet --version
```

### How do I install the template?

```bash
# From NuGet
dotnet new install Weda.Template

# From local source
dotnet new install ./WedaTemplate
```

---

## Project Creation

### How do I create a new project?

**Interactive mode (recommended):**
```bash
./create-project.sh   # Mac/Linux
.\create-project.ps1  # Windows
```

**Command line:**
```bash
dotnet new weda -n MyProject
```

### What are all the available parameters?

| Parameter | Description | Default |
|-----------|-------------|---------|
| `-n, --name` | Project name | (required) |
| `-db, --database` | Database provider (sqlite, postgres, mongo, none) | sqlite |
| `-N, --Nats` | NATS service name | weda-template |
| `--test` | Include test projects | true |
| `--wiki` | Include wiki generator | true |
| `--sample` | Include sample module | true |

### Can I change the database after project creation?

Yes, but it requires manual steps:

1. Update `appsettings.json` with new provider and connection string
2. Update NuGet packages if needed
3. Regenerate migrations
4. Update docker-compose.yml

---

## Database

### How do I switch from SQLite to PostgreSQL?

1. Update `appsettings.json`:
   ```json
   "Database": {
     "Provider": "PostgreSql",
     "ConnectionString": "Host=localhost;Database=mydb;Username=postgres;Password=postgres"
   }
   ```

2. Start PostgreSQL (via Docker):
   ```bash
   docker run -d --name postgres \
     -e POSTGRES_PASSWORD=postgres \
     -p 5432:5432 \
     postgres:16-alpine
   ```

3. Run migrations:
   ```bash
   dotnet ef database update --project src/MyProject.Infrastructure
   ```

### How do I run migrations?

```bash
# Create a migration
dotnet ef migrations add InitialCreate \
  --project src/MyProject.Infrastructure \
  --startup-project src/MyProject.Api

# Apply migrations
dotnet ef database update \
  --project src/MyProject.Infrastructure \
  --startup-project src/MyProject.Api
```

### Does MongoDB require migrations?

No, MongoDB is schema-less. The application will create collections automatically.

---

## NATS

### Do I need to install NATS separately?

Yes, NATS is not included in the template. Install it via Docker:

```bash
docker run -d --name nats \
  -p 4222:4222 \
  -p 8222:8222 \
  nats:latest -js
```

Or use the provided docker-compose (add NATS service):

```yaml
services:
  nats:
    image: nats:latest
    command: -js
    ports:
      - "4222:4222"
      - "8222:8222"
```

### What is JetStream?

JetStream is NATS's persistence layer that provides:
- Message persistence
- At-least-once delivery
- Consumer acknowledgments
- Stream replay

### How do I publish domain events?

```csharp
// In your domain entity
public void UpdateName(string newName)
{
    Name = newName;
    AddDomainEvent(new NameUpdatedEvent(Id, newName));
}

// Events are automatically published after SaveChanges
```

---

## Architecture

### What is Clean Architecture?

Clean Architecture separates concerns into layers:

- **Domain**: Business entities and logic (no dependencies)
- **Application**: Use cases and orchestration
- **Infrastructure**: Database, external services
- **API**: HTTP endpoints, controllers

### What is CQRS?

Command Query Responsibility Segregation separates:
- **Commands**: Operations that change state
- **Queries**: Operations that read state

```csharp
// Command
public record CreateEmployeeCommand(string Name) : IRequest<Result<Guid>>;

// Query
public record GetEmployeeQuery(Guid Id) : IRequest<Result<EmployeeDto>>;
```

### What is the Result pattern?

Instead of throwing exceptions, operations return Result objects:

```csharp
public Result<Employee> Create(string name)
{
    if (string.IsNullOrEmpty(name))
        return Result.Failure<Employee>(DomainErrors.InvalidName);

    return Result.Success(new Employee(name));
}
```

---

## Testing

### How do I run tests?

```bash
# All tests
dotnet test

# Specific project
dotnet test tests/MyProject.Domain.UnitTests

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

### How do I write integration tests?

Use the provided `WebApplicationFactory`:

```csharp
public class EmployeeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EmployeeTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateEmployee_ReturnsSuccess()
    {
        var response = await _client.PostAsJsonAsync("/api/employees",
            new { Name = "John" });

        response.EnsureSuccessStatusCode();
    }
}
```

---

## Docker

### How do I run with Docker Compose?

```bash
docker compose up --build
```

### How do I access the API in Docker?

The API is exposed on port 8080:
- Swagger: http://localhost:8080/swagger
- API: http://localhost:8080/api

### How do I view logs?

```bash
docker compose logs -f api
```

---

## Troubleshooting

### Build fails with "SDK not found"

Ensure you have .NET 10.0 SDK installed:

```bash
dotnet --list-sdks
```

### "Connection refused" when connecting to database

1. Check if the database is running
2. Verify connection string in `appsettings.json`
3. For Docker, ensure containers are on the same network

### NATS connection errors

1. Verify NATS is running: `docker ps | grep nats`
2. Check NATS URL in configuration
3. Ensure JetStream is enabled (`-js` flag)

### Swagger doesn't show endpoints

1. Ensure controllers have `[ApiController]` attribute
2. Check if routes are correctly configured
3. Verify the API is running in Development mode

---

## Getting Help

- **Documentation**: See `docs/` folder
- **Issues**: Open a GitHub issue
- **Discussions**: Use GitHub Discussions for questions