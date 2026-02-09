using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Weda.Template.Contracts.Employees;
using Weda.Template.Domain.Employees.Enums;
using Weda.Core.Api;
using Weda.Template.Contracts.Employees.Requests;
using Weda.Template.Contracts.Employees.Commands;
using Weda.Template.Contracts.Employees.Queries;
using Weda.Template.Api.Employees.Mappings;

namespace Weda.Template.Api.Employees.Controllers;

/// <summary>
/// Manages employee operations including CRUD and organizational hierarchy.
/// </summary>
[ApiVersion("1.0")]
public class EmployeesController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Retrieves all employees.
    /// </summary>
    /// <returns>A list of all employees in the system.</returns>
    /// <response code="200">Returns the list of employees.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeResponse>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var query = new ListEmployeesQuery();
        var result = await mediator.Send(query);

        return result.Match(
            employees => Ok(EmployeeMapper.ToResponseList(employees)),
            errors => Problem(errors));
    }

    /// <summary>
    /// Retrieves a specific employee by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the employee.</param>
    /// <returns>The employee details.</returns>
    /// <response code="200">Returns the employee.</response>
    /// <response code="404">Employee not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetEmployeeQuery(id);
        var result = await mediator.Send(query);

        return result.Match(
            employee => Ok(EmployeeMapper.ToResponse(employee)),
            errors => Problem(errors));
    }

    /// <summary>
    /// Creates a new employee.
    /// </summary>
    /// <param name="request">The employee creation details.</param>
    /// <returns>The created employee.</returns>
    /// <response code="201">Employee created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
    {
        var command = new CreateEmployeeCommand(
            request.Name,
            request.Email,
            request.Department,
            request.Position,
            request.HireDate,
            request.SupervisorId);

        var result = await mediator.Send(command);

        return result.Match(
            employee => CreatedAtAction(
                nameof(GetById),
                new { id = employee.Id },
                EmployeeMapper.ToResponse(employee)),
            errors => Problem(errors));
    }

    /// <summary>
    /// Updates an existing employee.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to update.</param>
    /// <param name="request">The updated employee details.</param>
    /// <returns>The updated employee.</returns>
    /// <response code="200">Employee updated successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Employee not found.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest request)
    {
        if (!Enum.TryParse<EmployeeStatus>(request.Status, ignoreCase: true, out var status))
        {
            return BadRequest($"Invalid status: {request.Status}");
        }

        var command = new UpdateEmployeeCommand(
            id,
            request.Name,
            request.Email,
            request.Department,
            request.Position,
            status,
            request.SupervisorId);

        var result = await mediator.Send(command);

        return result.Match(
            employee => Ok(EmployeeMapper.ToResponse(employee)),
            errors => Problem(errors));
    }

    /// <summary>
    /// Deletes an employee.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Employee deleted successfully.</response>
    /// <response code="404">Employee not found.</response>
    /// <response code="409">Cannot delete employee with subordinates.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteEmployeeCommand(id);
        var result = await mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    /// <summary>
    /// Retrieves all subordinates (direct and indirect reports) of an employee.
    /// </summary>
    /// <param name="id">The unique identifier of the supervisor.</param>
    /// <returns>A list of all subordinates.</returns>
    /// <response code="200">Returns the list of subordinates.</response>
    /// <response code="404">Supervisor not found.</response>
    [HttpGet("{id:int}/subordinates")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubordinates(int id)
    {
        var query = new GetSubordinatesQuery(id);
        var result = await mediator.Send(query);

        return result.Match(
            subordinates => Ok(EmployeeMapper.ToResponseList(subordinates)),
            errors => Problem(errors));
    }
}
