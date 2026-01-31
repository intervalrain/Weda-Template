using Weda.Template.Domain.Employees.DomainServices;
using Weda.Template.Domain.Employees.Repositories;
using Weda.Template.Infrastructure.Employees.Persistence;

using Microsoft.Extensions.DependencyInjection;

namespace Weda.Template.Infrastructure.Employees;

public static class EmployeesInfrastructureModule
{
    public static IServiceCollection AddEmployeesInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<EmployeeHierarchyManager>();

        return services;
    }
}
