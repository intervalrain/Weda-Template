using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Weda.Core.Api;
using Weda.Template.Application.Auth.Queries.GetToken;
using Weda.Template.Contracts.Auth;
using Weda.Template.Contracts.Auth.Commands;
using Weda.Template.Contracts.Auth.Requests;

namespace Weda.Template.Api.Auth.Controllers;

/// <summary>
/// Authentication and authorization operations.
/// </summary>
[AllowAnonymous]
[ApiVersion("1.0")]
public class AuthController(ISender _mediator) : ApiController
{
    /// <summary>
    /// Login with email and password.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <returns>The authentication response with JWT token.</returns>
    /// <response code="200">Login successful, returns JWT token.</response>
    /// <response code="401">Invalid credentials or account not active.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);

        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Generate a JWT token (for development/testing).
    /// </summary>
    /// <param name="request">The request to get JWT token.</param>
    /// <returns>The generated JWT token.</returns>
    /// <response code="200">Returns the JWT token.</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateToken(GetAuthRequest request)
    {
        var query = new GetTokenQuery(
            request.Id,
            request.Name,
            request.Email,
            request.Permissions,
            request.Roles);

        var result = await _mediator.Send(query);

        return result.Match(Ok, Problem);
    }
}