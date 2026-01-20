using Microsoft.EntityFrameworkCore;

using Weda.Template.Domain.Common;
using Weda.Template.Domain.Common.Persistence;

namespace Weda.Template.Infrastructure.Common.Persistence;

public class GenericRepository<T>(AppDbContext _dbContext) : IRepository<T>
    where T : Entity
{
    protected readonly DbSet<T> DbSet = _dbContext.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
