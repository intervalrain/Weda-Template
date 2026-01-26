namespace Weda.Template.Contracts.Users.Requests;

public record CreateUserRequest(
    string Email,
    string Password,
    string Name,
    List<string>? Roles = null,
    List<string>? Permissions = null);
