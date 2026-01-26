using Microsoft.EntityFrameworkCore;

using Weda.Core.Infrastructure.Persistence;
using Weda.Template.Domain.Users.Entities;
using Weda.Template.Domain.Users.Repositories;
using Weda.Template.Domain.Users.ValueObjects;
using Weda.Template.Infrastructure.Common.Persistence;

namespace Weda.Template.Infrastructure.Users.Persistence;

public class UserRepository(AppDbContext dbContext)
    : GenericRepository<User, Guid, AppDbContext>(dbContext), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailResult = UserEmail.Create(email);
        if (emailResult.IsError)
        {
            return null;
        }

        var userEmail = emailResult.Value;
        return await DbSet.FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailResult = UserEmail.Create(email);
        if (emailResult.IsError)
        {
            return false;
        }

        var userEmail = emailResult.Value;
        return await DbSet.AnyAsync(u => u.Email == userEmail, cancellationToken);
    }
}
