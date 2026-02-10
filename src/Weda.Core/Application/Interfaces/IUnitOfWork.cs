namespace Weda.Core.Application.Interfaces;

public interface IUnitOfWork
{
    public Task SaveChangesAsync();
}