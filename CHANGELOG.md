# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Interactive CLI tool (`create-project.sh`, `create-project.ps1`) for step-by-step project creation
- GitHub Actions CI/CD workflows for build, test, and NuGet publishing
- IDE host configuration for Visual Studio integration
- Environment variables example (`.env.example`)
- Comprehensive template parameter documentation

### Changed
- Improved docker-compose configurations for each database provider
- Updated VS Code settings with correct project names

### Fixed
- Template conditional directives for test and wiki projects in solution file

## [1.0.0] - 2026-02-17

### Added
- Initial release of Weda Clean Architecture Template
- **Architecture**
  - Clean Architecture with 5-layer structure (Api, Application, Domain, Contracts, Infrastructure)
  - Domain-Driven Design (DDD) patterns
  - CQRS with MediatR
  - Result pattern for error handling
  - Unit of Work pattern

- **Database Support**
  - SQLite (default)
  - PostgreSQL
  - MongoDB
  - InMemory (for testing)

- **NATS Integration**
  - JetStream for event-driven architecture
  - KV Store for caching
  - Object Store for file storage
  - NAK + Dead Letter Queue for resilience
  - SAGA pattern implementation

- **Observability**
  - OpenTelemetry integration
  - Serilog structured logging
  - Health checks

- **Developer Experience**
  - Swagger/OpenAPI documentation
  - Docker Compose support
  - Wiki documentation generator
  - Sample Employee module

- **Testing**
  - Unit tests for all layers
  - Integration tests
  - Test utilities library

### Template Parameters
- `-db` / `--database`: Database provider selection
- `-N` / `--Nats`: NATS service name
- `--test`: Include/exclude test projects
- `--wiki`: Include/exclude wiki generator
- `--sample`: Include/exclude sample module

---

## Version History Summary

| Version | Date | Highlights |
|---------|------|------------|
| 1.0.0 | TBD | Initial release with Clean Architecture, DDD, CQRS, NATS |

---

## Migration Guide

### From Pre-release to 1.0.0

If you were using a pre-release version:

1. Create a new project with the 1.0.0 template
2. Copy your domain entities and value objects
3. Migrate your application services and handlers
4. Update your infrastructure implementations
5. Run tests to verify everything works

---

## Roadmap

### Planned for Future Releases

- [ ] GraphQL support option
- [ ] gRPC service template
- [ ] Azure Service Bus integration
- [ ] RabbitMQ integration
- [ ] Multi-tenancy support
- [ ] Event sourcing option
