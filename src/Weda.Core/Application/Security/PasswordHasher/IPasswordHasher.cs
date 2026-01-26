namespace Weda.Core.Application.Security.PasswordHasher;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
