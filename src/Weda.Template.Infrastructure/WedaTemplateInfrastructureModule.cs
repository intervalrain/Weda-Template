using Weda.Core.Application.Interfaces;
using Weda.Core.Application.Security;
using Weda.Template.Domain.Users.Repositories;
using Weda.Template.Infrastructure.Common.Persistence;
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
using Weda.Core.Application.Security.Models;



#if sample
using Weda.Template.Infrastructure.Employees;
#endif

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
                .AddAuthorizationPolicies();
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
#if sqlite
            dbOptions.UseSqlite(options.ConnectionString);
#elif postgres
            dbOptions.UseNpgsql(options.ConnectionString);
#elif mongo
            dbOptions.UseMongoDB(options.ConnectionString, options.DatabaseName);
#elif nodb
            dbOptions.UseInMemoryDatabase(options.DatabaseName)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
#endif
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

#if sample
        services.AddEmployeesInfrastructure();
#endif

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, InfraPasswordHasher>();
        services.AddScoped<AppDbContextSeeder>();

        return services;
    }

    private static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<ICurrentUserProvider, InfraCurrentUserProvider>();
        services.AddSingleton<IPolicyEnforcer, PolicyEnforcer>();

        services.AddAuthorizationBuilder()
            .AddPolicy(Policy.AdminOrAbove, policy =>
                policy.RequireRole(Role.Admin, Role.SuperAdmin))
            .AddPolicy(Policy.SuperAdminOnly, policy =>
                policy.RequireRole(Role.SuperAdmin));

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
