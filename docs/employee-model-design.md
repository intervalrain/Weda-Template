# Employee Model Design

This document describes the Employee domain model design for the WEDA Template project.

## Entity Relationship Model (ERM)

```
+------------------+
|    Employee      |
+------------------+
| PK  Id           |  GUID
|     Name         |  VARCHAR(100)
|     Email        |  VARCHAR(256) UNIQUE
|     Department   |  VARCHAR(50)
|     Position     |  VARCHAR(100)
|     HireDate     |  DATETIME
|     Status       |  VARCHAR(20)
| FK  SupervisorId |  GUID (nullable) --+
|     CreatedAt    |  DATETIME          |
|     UpdatedAt    |  DATETIME (null)   |
+------------------+                    |
         ^                              |
         |  Self-Reference (0..1)       |
         +------------------------------+
```

## Table: Employees

| Column       | Type          | Constraints                    | Description                        |
|--------------|---------------|--------------------------------|------------------------------------|
| Id           | GUID          | PRIMARY KEY                    | Unique identifier                  |
| Name         | VARCHAR(100)  | NOT NULL                       | Employee full name                 |
| Email        | VARCHAR(256)  | NOT NULL, UNIQUE               | Employee email address             |
| Department   | VARCHAR(50)   | NOT NULL                       | Department enum (string)           |
| Position     | VARCHAR(100)  | NOT NULL                       | Job title/position                 |
| HireDate     | DATETIME      | NOT NULL                       | Employment start date              |
| Status       | VARCHAR(20)   | NOT NULL, DEFAULT 'Active'     | Employment status enum (string)    |
| SupervisorId | GUID          | FOREIGN KEY, NULLABLE          | Self-reference to supervisor       |
| CreatedAt    | DATETIME      | NOT NULL                       | Record creation timestamp          |
| UpdatedAt    | DATETIME      | NULLABLE                       | Last modification timestamp        |

## Relationships

### Self-Referencing Hierarchy (SupervisorId)

- **Type**: Many-to-One (self-reference)
- **Description**: Each employee can have one supervisor (except CEO/top-level)
- **FK Constraint**: `SupervisorId` references `Employees.Id`
- **Delete Behavior**: `SET NULL` - When supervisor is deleted, subordinates' SupervisorId becomes NULL

```
CEO (SupervisorId = NULL)
├── Manager A (SupervisorId = CEO.Id)
│   ├── Employee 1 (SupervisorId = Manager A.Id)
│   └── Employee 2 (SupervisorId = Manager A.Id)
└── Manager B (SupervisorId = CEO.Id)
    └── Employee 3 (SupervisorId = Manager B.Id)
```

## Enumerations

### Department

| Value          | Description                |
|----------------|----------------------------|
| Engineering    | Engineering department     |
| HumanResources | Human Resources department |
| Finance        | Finance department         |
| Marketing      | Marketing department       |
| Sales          | Sales department           |
| Operations     | Operations department      |

### EmployeeStatus

| Value    | Description                          |
|----------|--------------------------------------|
| Active   | Currently employed and working       |
| OnLeave  | Temporarily on leave                 |
| Inactive | No longer employed (resigned/fired)  |

## Value Objects

### EmployeeName

- **Max Length**: 100 characters
- **Validation**: Cannot be empty or whitespace
- **Storage**: Trimmed and stored as-is

### Email

- **Max Length**: 256 characters
- **Validation**:
  - Cannot be empty or whitespace
  - Must match email regex pattern: `^[^@\s]+@[^@\s]+\.[^@\s]+$`
- **Storage**: Trimmed and converted to lowercase

## Domain Services

### EmployeeHierarchyService

Handles hierarchy-related operations:

| Method                    | Description                                         |
|---------------------------|-----------------------------------------------------|
| AssignSupervisorAsync     | Assigns supervisor with circular reference check    |
| GetManagementChainAsync   | Returns all supervisors up the hierarchy            |
| GetAllReportsAsync        | Returns all direct and indirect subordinates        |

## Business Rules

1. **Self-Supervisor Prevention**: An employee cannot be their own supervisor
2. **Circular Reference Prevention**: Supervisor assignments are validated to prevent loops
3. **Inactive Employee Modification**: Inactive employees cannot be modified (except reactivation)
4. **Subordinate Deletion Protection**: Employees with subordinates cannot be deleted
5. **Email Uniqueness**: Each employee must have a unique email address

## API Endpoints

| Method | Endpoint                          | Description                    |
|--------|-----------------------------------|--------------------------------|
| GET    | /api/v1/employees                 | List all employees             |
| GET    | /api/v1/employees/{id}            | Get employee by ID             |
| POST   | /api/v1/employees                 | Create new employee            |
| PUT    | /api/v1/employees/{id}            | Update employee                |
| DELETE | /api/v1/employees/{id}            | Delete employee                |
| GET    | /api/v1/employees/{id}/subordinates | Get all subordinates         |
