using System.Reflection;
using Asp.Versioning;
using EdgeSync.ServiceFramework.Core;
using EdgeSync.ServiceFramework.DependencyInjection;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Weda.Core.Api.Swagger;
using Weda.Core.Application.Behaviors;
using Weda.Core.Infrastructure.Middleware;

namespace Weda.Core;

public static class WedaCoreModule
{
    public static IServiceCollection AddWedaCore<TAssemblyMarker, TContractsMarker, TApplicationMarker>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IServiceCollection> configureMediator,
        Action<WedaCoreOptions>? configure = null)
    {
        var options = new WedaCoreOptions();
        configure?.Invoke(options);

        services.AddProblemDetails();

        configureMediator(services);

        if (options.Messaging.Enabled)
        {
            services.AddMessaging(configuration);
        }

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddValidatorsFromAssemblyContaining<TApplicationMarker>();

        services.AddPresentation<TContractsMarker>(options);
        services.AddDistributedEventHandlers<TAssemblyMarker>();

        return services;
    }

    public static void AddWedaCoreSwagger(this SwaggerGenOptions options, OpenApiInfo info)
    {
        options.SwaggerDoc(info.Version, info);

        options.SchemaFilter<ResponseExampleSchemaFilter>();

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme.\n\n" +
                          "Enter your token in the text input below.\n\n" +
                          "Example: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        });

        options.OperationFilter<AuthorizeCheckOperationFilter>();
    }

    public static WebApplication UseWedaCore<TDbContext>(
        this WebApplication app,
        Action<WedaCoreAppOptions>? configure = null)
        where TDbContext : DbContext
    {
        var options = new WedaCoreAppOptions();
        configure?.Invoke(options);

        if (options.EnsureDatabaseCreated)
        {
            app.EnsureDatabaseCreated<TDbContext>();
        }

        app.UseInfrastructure<TDbContext>();
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(uiOptions =>
            {
                uiOptions.SwaggerEndpoint(options.SwaggerEndpointUrl, options.SwaggerEndpointName);
                uiOptions.RoutePrefix = options.RoutePrefix;

                options.ConfigureSwaggerUI?.Invoke(uiOptions);
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

    /// <summary>
    /// Scans the specified assembly for all types that inherit from DistributedEventHandler
    /// and registers them as hosted services.
    /// </summary>
    /// <typeparam name="TMarker">A marker type in the assembly to scan.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection AddDistributedEventHandlers<TMarker>(this IServiceCollection services)
    {
        return services.AddDistributedEventHandlers(typeof(TMarker).Assembly);
    }

    /// <summary>
    /// Scans the specified assembly for all types that inherit from DistributedEventHandler
    /// and registers them as hosted services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection AddDistributedEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract &&
                        !t.IsInterface &&
                        IsDistributedEventHandler(t));

        foreach (var handlerType in handlerTypes)
        {
            services.AddSingleton(typeof(IHostedService), handlerType);
        }

        return services;
    }

    private static IApplicationBuilder EnsureDatabaseCreated<TDbContext>(this IApplicationBuilder app)
        where TDbContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        dbContext.Database.EnsureCreated();
        return app;
    }

    private static IServiceCollection AddPresentation<TExamplesMarker>(
        this IServiceCollection services,
        WedaCoreOptions options)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddApiVersioning(versioningOptions =>
        {
            versioningOptions.DefaultApiVersion = options.DefaultApiVersion;
            versioningOptions.AssumeDefaultVersionWhenUnspecified = true;
            versioningOptions.ReportApiVersions = true;
            versioningOptions.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(explorerOptions =>
        {
            explorerOptions.GroupNameFormat = options.ApiVersionGroupNameFormat;
            explorerOptions.SubstituteApiVersionInUrl = true;
        });

        services.AddSwaggerExamplesFromAssemblyOf<TExamplesMarker>();

        services.AddSwaggerGen(swaggerOptions =>
        {
            foreach (var assembly in options.XmlCommentAssemblies)
            {
                var xmlFilename = $"{assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                if (File.Exists(xmlPath))
                {
                    swaggerOptions.IncludeXmlComments(xmlPath);
                }
            }

            swaggerOptions.ExampleFilters();
            swaggerOptions.AddWedaCoreSwagger(options.OpenApiInfo);

            options.ConfigureSwagger?.Invoke(swaggerOptions);
        });

        return services;
    }

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ServiceFramework 內建支援從 NatsApi section 讀取設定
        services.AddServiceFramework(configuration);

        return services;
    }

    private static IApplicationBuilder UseInfrastructure<TDbContext>(this IApplicationBuilder app)
        where TDbContext : DbContext
    {
        app.UseMiddleware<EventualConsistencyMiddleware<TDbContext>>();

        return app;
    }

    private static bool IsDistributedEventHandler(Type type)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition().Name.StartsWith("DistributedEventHandler"))
            {
                return true;
            }

            if (current == typeof(BaseEventHandler))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }
}
