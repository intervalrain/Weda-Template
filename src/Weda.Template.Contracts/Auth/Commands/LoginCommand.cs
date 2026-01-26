using ErrorOr;
using Mediator;
using Weda.Template.Contracts.Auth;

namespace Weda.Template.Contracts.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password) : IRequest<ErrorOr<AuthResponse>>;
