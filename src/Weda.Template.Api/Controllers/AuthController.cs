using Asp.Versioning;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Weda.Core.Api;
using Weda.Template.Application.Auth.Queries.GetToken;
using Weda.Template.Contracts.Auth;

namespace Weda.Template.Api.Controllers;

[AllowAnonymous]
[ApiVersion("1.0")]
public class AuthController(ISender _mediator) : ApiController
{
    [HttpPost("generate")]
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