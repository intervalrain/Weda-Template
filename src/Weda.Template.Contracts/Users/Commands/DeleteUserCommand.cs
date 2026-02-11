using ErrorOr;
using Weda.Core.Application.Interfaces;

namespace Weda.Template.Contracts.Users.Commands;

public record DeleteUserCommand(Guid Id) : ICommand<ErrorOr<Deleted>>;
