using Weda.Core.Application.Interfaces;
using Weda.Core.Application.Security;
using Weda.Core.Application.Security.CurrentUserProvider;
using Weda.Core.Application.Security.PasswordHasher;
using Weda.Template.Domain.Employees.DomainServices;
using Weda.Template.Domain.Employees.Repositories;
using Weda.Template.Domain.Users.Repositories;
using Weda.Template.Infrastructure.Common.Persistence;
using Weda.Template.Infrastructure.Employees.Persistence;
using Weda.Template.Infrastructure.Persistence;
using Weda.Template.Infrastructure.Security;
using Weda.Template.Infrastructure.Security.PolicyEnforcer;
using Weda.Template.Infrastructure.Security.TokenValidation;
using Weda.Template.Infrastructure.Services;
using Weda.Template.Infrastructure.Users.Persistence;

using InfraCurrentUserProvider = Weda.Template.Infrastructure.Security.CurrentUserProvider.CurrentUserProvider;
using InfraPasswordHasher = Weda.Template.Infrastructure.Security.PasswordHasher.BCryptPasswordHasher;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Weda.Template.Infrastructure;

public static class WedaTemplateInfrastructureModule
{
    private const string DatabaseSection = "Database";
    private const string AuthenticationSection = "Authentication";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseSection).Get<DatabaseOptions>() ?? new DatabaseOptions();
        var authOptions = configuration.GetSection(AuthenticationSection).Get<AuthenticationOptions>() ?? new AuthenticationOptions();

        services
            .Configure<DatabaseOptions>(configuration.GetSection(DatabaseSection))
            .Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationSection))
            .AddHttpContextAccessor()
            .AddServices()
            .AddPersistence(databaseOptions);

        if (authOptions.Enabled)
        {
            services
                .AddJwtAuthentication(configuration)
                .AddAuthorization();
        }

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, DatabaseOptions options)
    {
        services.AddDbContext<AppDbContext>(dbOptions =>
        {
            switch (options.Provider)
            {
                case DatabaseProvider.Sqlite:
                    dbOptions.UseSqlite(options.ConnectionString);
                    break;
                case DatabaseProvider.PostgreSql:
                    dbOptions.UseNpgsql(options.ConnectionString);
                    break;
                case DatabaseProvider.MongoDb:
                    dbOptions.UseMongoDB(options.ConnectionString, options.DatabaseName);
                    break;
                case DatabaseProvider.InMemory:
                    dbOptions.UseInMemoryDatabase(options.DatabaseName);
                    break;
            }
        });

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<EmployeeHierarchyManager>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, InfraPasswordHasher>();
        services.AddScoped<AppDbContextSeeder>();

        return services;
    }

    private static IServiceCollection AddAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<ICurrentUserProvider, InfraCurrentUserProvider>();
        services.AddSingleton<IPolicyEnforcer, PolicyEnforcer>();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Section));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services
            .ConfigureOptions<JwtBearerTokenValidationConfiguration>()
            .AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        return services;
    }
}
