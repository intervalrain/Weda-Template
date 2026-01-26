using Weda.Core.Domain;
using Weda.Template.Domain.Users.Entities;

namespace Weda.Template.Domain.Users.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
}
