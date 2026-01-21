using System.Reflection;
using System.Runtime.CompilerServices;

using Asp.Versioning;

using EdgeSync.ServiceFramework.DependencyInjection;

using FluentValidation;

using Mediator;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

using Weda.Core.Api.Swagger;
using Weda.Core.Application.Behaviors;
using Weda.Core.Infrastructure.Messaging;
using Weda.Core.Infrastructure.Middleware;

namespace Weda.Core;

public static class WedaCoreModule
{
    public static IServiceCollection AddWedaCore<TContractsMarker, TApplicationMarker>(
        this IServiceCollection services,
        Action<IServiceCollection> configureMediator,
        Action<WedaCoreOptions>? configure = null)
    {
        var options = new WedaCoreOptions();
        configure?.Invoke(options);

        services.AddProblemDetails();

        configureMediator(services);

        if (options.Messaging.Enabled)
        {
            services.AddMessaging(options.Messaging);
        }

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddValidatorsFromAssemblyContaining<TApplicationMarker>();

        services.AddPresentation<TContractsMarker>(options);

        return services;
    }

    public static void AddWedaCoreSwagger(this SwaggerGenOptions options)
    {
        options.SchemaFilter<ResponseExampleSchemaFilter>();
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
        app.UseAuthorization();
        app.MapControllers();

        return app;
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
            swaggerOptions.AddWedaCoreSwagger();

            options.ConfigureSwagger?.Invoke(swaggerOptions);
        });

        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services, WedaMessagingOptions options)
    {
        services.AddServiceFramework(sfOptions =>
        {
            sfOptions.DefaultConnection = options.DefaultConnection;

            foreach (var conn in options.Connections)
            {
                var builder = sfOptions.AddConnection(conn.Name, conn.Url);
                if (!string.IsNullOrEmpty(conn.CredFile))
                {
                    builder.WithCredFile(conn.CredFile);
                }
            }
        });

        return services;
    }

    private static IApplicationBuilder UseInfrastructure<TDbContext>(this IApplicationBuilder app)
        where TDbContext : DbContext
    {
        app.UseMiddleware<EventualConsistencyMiddleware<TDbContext>>();

        return app;
    }
}
