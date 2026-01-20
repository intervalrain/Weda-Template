# Leave Request Feature Design

This document describes the proposed Leave Request feature for extending the WEDA Template with a more complex DDD scenario.

## User Stories

1. As an employee, I want to submit a leave request specifying leave type, dates, and reason
2. As a supervisor, I want to approve/reject leave requests from my direct reports
3. As an employee, I want to cancel my pending leave request
4. As the system, when leave is approved and covers today, the employee status should change to OnLeave

## Entity Relationship Model

```
+------------------+          +-------------------+
|    Employee      |          |   LeaveRequest    |
+------------------+          +-------------------+
| PK  Id           |<------+  | PK  Id            |
|     Name         |       |  | FK  EmployeeId    |----+
|     Email        |       |  | FK  ApproverId    |----+ (must be Supervisor)
|     SupervisorId |       +--|     Status        |
|     Status       |          |     LeaveType     |
+------------------+          |     StartDate     |
                              |     EndDate       |
                              |     Reason        |
                              |     RequestedAt   |
                              |     ReviewedAt    |
                              |     ReviewComment |
                              +-------------------+
```

## Enumerations

### LeaveType

| Value    | Description              |
|----------|--------------------------|
| Annual   | Paid annual leave        |
| Sick     | Sick leave               |
| Personal | Personal leave           |
| Unpaid   | Unpaid leave             |

### LeaveStatus

| Value     | Description                    |
|-----------|--------------------------------|
| Pending   | Awaiting supervisor approval   |
| Approved  | Leave request approved         |
| Rejected  | Leave request rejected         |
| Cancelled | Cancelled by employee          |

## State Machine

```
                    +------------+
                    |  Pending   |
                    +------------+
                    /     |      \
                   /      |       \
                  v       v        v
         +----------+ +----------+ +-----------+
         | Approved | | Rejected | | Cancelled |
         +----------+ +----------+ +-----------+
              (final)    (final)      (final)
```

## Business Rules (Invariants)

1. **Approval Authority**: Only the employee's direct supervisor can approve/reject
2. **State Transitions**: Only Pending can transition to Approved/Rejected/Cancelled
3. **Date Validation**: EndDate >= StartDate, StartDate >= Today
4. **Cancel Restriction**: Only Pending requests can be cancelled, only by the requester
5. **No Self-Approval**: Employee cannot approve their own leave request

## Domain Events

| Event                       | Trigger                    | Possible Side Effects           |
|-----------------------------|----------------------------|---------------------------------|
| LeaveRequestSubmittedEvent  | Leave request created      | Notify supervisor               |
| LeaveRequestApprovedEvent   | Supervisor approves        | Update employee status if today |
| LeaveRequestRejectedEvent   | Supervisor rejects         | Notify employee                 |
| LeaveRequestCancelledEvent  | Employee cancels           | Notify supervisor               |

## Domain Service

### LeaveApprovalService

Handles the approval workflow:

| Method        | Description                                              |
|---------------|----------------------------------------------------------|
| ApproveAsync  | Validates supervisor authority, updates status           |
| RejectAsync   | Validates supervisor authority, updates status           |
| CancelAsync   | Validates requester identity, updates status             |

## API Endpoints (Proposed)

| Method | Endpoint                              | Description                    |
|--------|---------------------------------------|--------------------------------|
| GET    | /api/v1/leave-requests                | List all (with filters)        |
| GET    | /api/v1/leave-requests/{id}           | Get by ID                      |
| POST   | /api/v1/leave-requests                | Submit new request             |
| POST   | /api/v1/leave-requests/{id}/approve   | Approve request                |
| POST   | /api/v1/leave-requests/{id}/reject    | Reject request                 |
| POST   | /api/v1/leave-requests/{id}/cancel    | Cancel request                 |
| GET    | /api/v1/employees/{id}/leave-requests | Get employee's requests        |

## DDD Concepts Demonstrated

1. **Aggregate Root**: LeaveRequest as a separate aggregate with its own lifecycle
2. **Domain Events**: Cross-aggregate communication (LeaveRequest → Employee status)
3. **Domain Service**: LeaveApprovalService for complex business logic
4. **Value Objects**: DateRange for StartDate/EndDate validation
5. **Invariants**: Business rules enforced within the aggregate

## Implementation Notes

- LeaveRequest is a separate Aggregate (not part of Employee aggregate)
- Uses Domain Events to communicate with Employee aggregate
- Approval requires cross-aggregate validation (checking supervisor relationship)
