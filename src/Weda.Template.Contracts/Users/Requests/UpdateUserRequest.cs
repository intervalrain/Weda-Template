namespace Weda.Template.Contracts.Users.Requests;

public record UpdateUserRequest(
    string? Email = null,
    string? Name = null,
    string? Password = null);
