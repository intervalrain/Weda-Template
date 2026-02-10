using Microsoft.EntityFrameworkCore;

using Weda.Core.Infrastructure.Persistence;
using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Repositories;
using Weda.Template.Infrastructure.Common.Persistence;

namespace Weda.Template.Infrastructure.Employees.Persistence;

public class EmployeeRepository(AppDbContext dbContext) : GenericRepository<Employee, int, AppDbContext>(dbContext), IEmployeeRepository
{
    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Email.Value == email, cancellationToken);
    }

    public async Task<Employee?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Name.Value == name, cancellationToken);
    }

    public async Task<List<Employee>> GetBySupervisorIdAsync(int supervisorId, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(e => e.SupervisorId == supervisorId).ToListAsync(cancellationToken);
    }
}
