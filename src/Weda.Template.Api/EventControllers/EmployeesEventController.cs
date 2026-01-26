using Asp.Versioning;

using Weda.Core.Infrastructure.Nats;
using Weda.Core.Infrastructure.Nats.Attributes;
using Weda.Core.Infrastructure.Nats.Enums;
using Weda.Template.Contracts.Employees.Commands;
using Weda.Template.Contracts.Employees.Events;
using Weda.Template.Contracts.Employees.Queries;
using Weda.Template.Contracts.Employees.Requests;
using Weda.Template.Domain.Employees.Enums;

namespace Weda.Template.Api.EventControllers;

/// <summary>
/// Employee EventController demonstrating different NATS patterns.
/// This is a reference implementation showing how to use:
/// - Request-Reply (with response)
/// - JetStream Consume (continuous processing)
/// - JetStream Fetch (batch processing)
/// - Core Pub-Sub (fire-and-forget)
/// </summary>
[ApiVersion("1")]
public class EmployeeEventController : EventController
{
    /// <summary>
    /// Request-Reply pattern example with subject-based ID.
    /// The {id} placeholder extracts the employee ID from the subject.
    /// Subject pattern: employee.v1.{id}.get -> subscribes to employee.v1.*.get
    /// Example: Request to "employee.v1.123.get" extracts id=123
    /// </summary>
    [Subject("[controller].v{version:apiVersion}.{id}.get")]
    public async Task<GetEmployeeResponse> GetEmployee(int id)
    {
        Logger.LogInformation("Querying employee: {EmployeeId}", id);

        // Use Mediator to query employee from application layer
        var query = new GetEmployeeQuery(id);
        var result = await Mediator.Send(query);

        if (result.IsError)
        {
            Logger.LogWarning("Employee not found: {EmployeeId}", id);
            throw new InvalidOperationException($"Employee {id} not found");
        }

        var employee = result.Value;

        return new GetEmployeeResponse(
            employee.Id,
            employee.Name,
            employee.Email,
            employee.Department,
            employee.Position,
            employee.Status);
    }

    /// <summary>
    /// Get all employees.
    /// Returns a list of all employees in the system.
    /// </summary>
    [Subject("[controller].v{version:apiVersion}.getAll")]
    public async Task<ListEmployeesResponse> ListEmployees()
    {
        Logger.LogInformation("Querying employees");

        // Use Mediator to query employee from application layer
        var query = new ListEmployeesQuery();
        var result = await Mediator.Send(query);

        if (result.IsError)
        {
            Logger.LogError(result.FirstError.Description);
            throw new InvalidOperationException($"Code: {result.FirstError.Code}, Description: {result.FirstError.Description}");
        }

        var employees = result.Value.Select(e => new GetEmployeeResponse(
            e.Id,
            e.Name,
            e.Email,
            e.Department,
            e.Position,
            e.Status)).ToList();

        return new ListEmployeesResponse(employees);
    }

    /// <summary>
    /// Create a new employee.
    /// Adds a new employee to the system with the provided details.
    /// </summary>
    [Subject("[controller].v{version:apiVersion}.create")]
    public async Task<GetEmployeeResponse> CreateEmployee(CreateEmployeeRequest request)
    {
        Logger.LogInformation("Creating employee: {Name}", request.Name);

        // Use Mediator to query employee from application layer
        var command = new CreateEmployeeCommand(
            request.Name,
            request.Email,
            request.Department,
            request.Position,
            request.HireDate,
            request.SupervisorId);
        var result = await Mediator.Send(command);

        if (result.IsError)
        {
            Logger.LogError(result.FirstError.Description);
            throw new InvalidOperationException($"Code: {result.FirstError.Code}, Description: {result.FirstError.Description}");
        }

        var employee = result.Value;

        return new GetEmployeeResponse(
            employee.Id,
            employee.Name,
            employee.Email,
            employee.Department,
            employee.Position,
            employee.Status);
    }

    /// <summary>
    /// Update an existing employee.
    /// Modifies the employee's information with the provided details.
    /// </summary>
    [Subject("[controller].v{version:apiVersion}.{id}.update")]
    public async Task<GetEmployeeResponse> UpdateEmployee(int id, UpdateEmployeeRequest request)
    {
        if (!Enum.TryParse<EmployeeStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new InvalidDataException("Invalid employee's status");
        }
        
        Logger.LogInformation("Updating employee: {Name}", request.Name);


        // Use Mediator to query employee from application layer
        var command = new UpdateEmployeeCommand(
            id,
            request.Name,
            request.Email,
            request.Department,
            request.Position,
            status,
            request.SupervisorId);
        var result = await Mediator.Send(command);

        if (result.IsError)
        {
            Logger.LogError(result.FirstError.Description);
            throw new InvalidOperationException($"Code: {result.FirstError.Code}, Description: {result.FirstError.Description}");
        }

        var employee = result.Value;

        return new GetEmployeeResponse(
            employee.Id,
            employee.Name,
            employee.Email,
            employee.Department,
            employee.Position,
            employee.Status);
    }

    /// <summary>
    /// Delete an employee.
    /// Removes the employee from the system permanently.
    /// </summary>
    [Subject("[controller].v{version:apiVersion}.{id}.delete")]
    public async Task<DeleteEmployeeResponse> DeleteEmployee(int id)
    {
        Logger.LogInformation("Deleting employee: {EmployeeId}", id);

        // Use Mediator to delete employee from application layer
        var command = new DeleteEmployeeCommand(id);
        var result = await Mediator.Send(command);

        if (result.IsError)
        {
            Logger.LogError("Failed to delete employee {EmployeeId}: {Error}", id, result.FirstError.Description);
            throw new InvalidOperationException($"Code: {result.FirstError.Code}, Description: {result.FirstError.Description}");
        }

        return new DeleteEmployeeResponse(id, true, $"Employee {id} deleted successfully");
    }

    /// <summary>
    /// JetStream Consume pattern example (default).
    /// No return type (void/Task), so uses JetStream with ConsumeAsync.
    /// Subject: employee.v1_0.created
    /// Stream: employees_stream (from class-level attribute)
    /// Consumer: employee_handler (from class-level attribute).
    /// </summary>
    [Subject("[controller].v{version:apiVersion}.created")]
    public async Task OnEmployeeCreated(CreateEmployeeNatsEvent @event)
    {
        Logger.LogInformation(
            "Creating Employee with: {Name} in {Department} with Consume Mode",
            @event.Name,
            @event.Department);

        var command = new CreateEmployeeCommand(@event.Name, @event.Email, @event.Department, @event.Position, DateTime.UtcNow);
        await Mediator.Send(command);

        Logger.LogInformation("Employee created successfully");
    }

    /// <summary>
    /// JetStream Fetch pattern example.
    /// Uses ConsumerMode.Fetch for batch processing.
    /// Suitable for tasks that don't need immediate processing.
    /// Subject: employee.v1_0.export
    /// Note: Consumer name is overridden at method level using Consumer property.
    /// </summary>
    [Subject("[controller].v{version:apiVersion}.export", ConsumerMode = ConsumerMode.Fetch, Consumer = "employee_export_handler")]
    public async Task ExportEmployeeData(CreateEmployeeNatsEvent @event)
    {
        Logger.LogInformation(
            "Creating Employee with: {Name} in {Department} with Fetch Mode",
            @event.Name,
            @event.Department);

        var command = new CreateEmployeeCommand(@event.Name, @event.Email, @event.Department, @event.Position, DateTime.UtcNow);
        await Mediator.Send(command);

        Logger.LogInformation("Employee created successfully");
    }

    /// <summary>
    /// Core Pub-Sub pattern example demonstrating runtime subject manipulation.
    /// Subscribes to employee.v1.*.get, then publishes result to employee.v1.{id}.doc
    /// </summary>
    [Subject("[controller].v{version:apiVersion}.get", DeliveryMode = DeliveryMode.Core)]
    public async Task SendNotification()
    {
        var responseSubject = Subject.Replace(".get", ".doc");

        Logger.LogInformation("Received on {IncomingSubject}, publishing to {ResponseSubject}", Subject, responseSubject);

        var query = new ListEmployeesQuery();
        var result = await Mediator.Send(query);

        if (!result.IsError)
        {
            // Publish directly - NatsJsonSerializerRegistry handles serialization
            await GetConnection("bus").PublishAsync(responseSubject, result.Value);
        }
    }
}
