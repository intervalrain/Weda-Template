# Template Parameters

This document describes all available parameters for the Weda Clean Architecture Template.

## Quick Start

```bash
# Basic usage with defaults (SQLite)
dotnet new weda -n MyProject

# With PostgreSQL
dotnet new weda -n MyProject -db postgres

# Minimal setup (no tests, no wiki, no sample)
dotnet new weda -n MyProject --test false --wiki false --sample false
```

## Parameters

### `-n, --name` (Required)
The name of your project. This will be used as the namespace and project file prefix.

```bash
dotnet new weda -n MyCompany.MyProject
```

### `-db, --database` (Default: `sqlite`)
Choose the database provider for your application.

| Value | Description | Use Case |
|-------|-------------|----------|
| `sqlite` | SQLite (default) | Development, small applications, embedded scenarios |
| `postgres` | PostgreSQL | Production, enterprise applications |
| `mongo` | MongoDB | Document-oriented, NoSQL requirements |
| `none` | InMemory | Testing, prototyping |

```bash
# SQLite (default)
dotnet new weda -n MyProject

# PostgreSQL
dotnet new weda -n MyProject -db postgres

# MongoDB
dotnet new weda -n MyProject -db mongo

# InMemory (no database)
dotnet new weda -n MyProject -db none
```

### `-N, --Nats` (Default: `weda-template`)
The NATS service name used for event-driven messaging and JetStream streams.

```bash
dotnet new weda -n MyProject --Nats my-service
```

This affects:
- NATS stream names
- Consumer group names
- KV bucket names
- Object store names

### `--test` (Default: `true`)
Include unit and integration test projects.

```bash
# Include tests (default)
dotnet new weda -n MyProject --test true

# Exclude tests
dotnet new weda -n MyProject --test false
```

Test projects included:
- `{Name}.Domain.UnitTests`
- `{Name}.Application.UnitTests`
- `{Name}.Infrastructure.UnitTests`
- `{Name}.Api.IntegrationTests`
- `{Name}.TestCommon`

### `--wiki` (Default: `true`)
Include wiki documentation and the WikiGenerator tool.

```bash
# Include wiki (default)
dotnet new weda -n MyProject --wiki true

# Exclude wiki
dotnet new weda -n MyProject --wiki false
```

When enabled:
- Includes `tools/WikiGenerator` project
- Includes `docs/wiki` documentation
- Adds wiki volume to docker-compose

### `--sample` (Default: `true`)
Include the Employee sample module as a reference implementation.

```bash
# Include sample (default)
dotnet new weda -n MyProject --sample true

# Exclude sample
dotnet new weda -n MyProject --sample false
```

The sample module demonstrates:
- Entity definition with domain events
- CQRS commands and queries
- Repository implementation
- API endpoints
- Unit tests

### `--skipRestore` (Default: `false`)
Skip the automatic NuGet restore after project creation.

```bash
# Skip restore
dotnet new weda -n MyProject --skipRestore true
```

## Common Scenarios

### Production-Ready Setup
```bash
dotnet new weda -n MyCompany.OrderService \
  -db postgres \
  --Nats order-service \
  --sample false
```

### Development/Learning Setup
```bash
dotnet new weda -n LearnCleanArch \
  -db sqlite \
  --sample true \
  --wiki true
```

### Minimal API Only
```bash
dotnet new weda -n MinimalApi \
  -db none \
  --test false \
  --wiki false \
  --sample false
```

### CI/CD Testing
```bash
dotnet new weda -n TestProject \
  -db sqlite \
  --skipRestore true
```

## Generated Structure

```
MyProject/
├── src/
│   ├── MyProject.Api/           # Web API layer
│   ├── MyProject.Application/   # Use cases, CQRS
│   ├── MyProject.Contracts/     # DTOs, interfaces
│   ├── MyProject.Domain/        # Entities, value objects
│   └── MyProject.Infrastructure/# Data access, external services
├── tests/                       # (when --test true)
│   ├── MyProject.Domain.UnitTests/
│   ├── MyProject.Application.UnitTests/
│   ├── MyProject.Infrastructure.UnitTests/
│   ├── MyProject.Api.IntegrationTests/
│   └── MyProject.TestCommon/
├── tools/                       # (when --wiki true)
│   └── WikiGenerator/
├── docs/                        # (when --wiki true)
│   └── wiki/
├── docker-compose.yml           # Database + API
├── Dockerfile
└── MyProject.sln
```

## Environment Variables

After generation, you can override settings via environment variables:

```bash
# Database
Database__Provider=PostgreSql
Database__ConnectionString=Host=localhost;Database=mydb;Username=user;Password=pass

# NATS
Nats__Url=nats://localhost:4222

# Logging
Serilog__MinimumLevel__Default=Debug
```

## See Also

- [README.md](../README.md) - Project overview
- [Architecture Guide](./wiki/architecture.md) - Detailed architecture documentation
- [NATS Integration](./wiki/nats.md) - NATS setup and usage