using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Weda.Core.Api;
using Weda.Template.Application.Auth.Queries.GetToken;
using Weda.Template.Contracts.Auth;

namespace Weda.Template.Api.Controllers;

/// <summary>
/// Generate JWT token to authorize.
/// </summary>
[AllowAnonymous]
[ApiVersion("1.0")]
public class AuthController(ISender _mediator) : ApiController
{
    /// <summary>
    /// Generate a JWT token all employees.
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