# Contributing to Weda Template

Thank you for your interest in contributing to Weda Template! This document provides guidelines and information for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Project Structure](#project-structure)
- [Making Changes](#making-changes)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Submitting Changes](#submitting-changes)

## Code of Conduct

Please be respectful and constructive in all interactions. We welcome contributors of all experience levels.

## Getting Started

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/weda-template.git
   cd weda-template
   ```
3. Add the upstream remote:
   ```bash
   git remote add upstream https://github.com/advantech/weda-template.git
   ```

## Development Setup

### Prerequisites

- .NET 10.0 SDK or later
- Your favorite IDE (VS Code, Visual Studio, Rider)
- Docker (optional, for testing database integrations)

### Building the Template

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Install template locally for testing
dotnet new install ./WedaTemplate
```

### Testing Template Generation

```bash
# Create a test project
mkdir /tmp/test-project
cd /tmp/test-project
dotnet new weda -n TestProject -db postgres --test false

# Verify it builds
cd TestProject
dotnet build
```

## Project Structure

```
WedaTemplate/
├── .template.config/       # Template configuration
│   ├── template.json       # Main template config
│   └── ide.host.json       # IDE integration
├── src/
│   ├── Weda.Template.Api/          # Web API layer
│   ├── Weda.Template.Application/  # Use cases, CQRS
│   ├── Weda.Template.Contracts/    # DTOs, interfaces
│   ├── Weda.Template.Domain/       # Entities, value objects
│   └── Weda.Template.Infrastructure/ # Data access
├── tests/                  # Test projects
├── tools/                  # Utility projects (WikiGenerator)
└── docs/                   # Documentation
```

## Making Changes

### Branch Naming

- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation updates
- `refactor/` - Code refactoring

Example: `feature/add-redis-cache`

### Commit Messages

Follow conventional commits:

```
type(scope): description

[optional body]

[optional footer]
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

Examples:
```
feat(template): add Redis cache option
fix(docker): correct PostgreSQL connection string
docs(readme): update installation instructions
```

## Coding Standards

### C# Guidelines

- Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful names for classes, methods, and variables
- Keep methods small and focused
- Use async/await for I/O operations
- Prefer records for DTOs

### Template Guidelines

- Use conditional directives (`#if`, `#endif`) for optional features
- Ensure all parameters have defaults
- Test all parameter combinations
- Update documentation when adding parameters

### Architecture Guidelines

- Follow Clean Architecture principles
- Keep domain logic in the Domain layer
- Use CQRS for commands and queries
- Implement the Result pattern for error handling

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Weda.Template.Domain.UnitTests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Requirements

- Unit tests for domain logic
- Integration tests for API endpoints
- Test all template parameter combinations

### Testing Template Changes

After modifying template.json or adding conditional logic:

```bash
# Test default parameters
dotnet new weda -n Test1 --skipRestore

# Test with different databases
dotnet new weda -n Test2 -db postgres --skipRestore
dotnet new weda -n Test3 -db mongo --skipRestore

# Test without optional features
dotnet new weda -n Test4 --test false --wiki false --sample false --skipRestore
```

## Submitting Changes

### Pull Request Process

1. Ensure all tests pass
2. Update documentation if needed
3. Update CHANGELOG.md
4. Create a pull request with a clear description
5. Wait for review

### Pull Request Checklist

- [ ] Tests pass locally
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] No breaking changes (or clearly documented)
- [ ] Template generation tested with various parameters

### Review Process

1. Maintainers will review your PR
2. Address any feedback
3. Once approved, your PR will be merged

## Questions?

Feel free to open an issue for questions or discussions.

Thank you for contributing! 🎉