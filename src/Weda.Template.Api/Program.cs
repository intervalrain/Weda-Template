using System.Reflection;
using Mediator;
using Weda.Core;
using Weda.Template.Application;
using Weda.Template.Contracts;
using Weda.Template.Infrastructure;
using Weda.Template.Infrastructure.Common.Persistence;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddWedaCore<IContractsMarker, IApplicationMarker>(
            services => services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.Assemblies = [typeof(IApplicationMarker).Assembly];
            }),
            options =>
            {
                options.XmlCommentAssemblies = [Assembly.GetExecutingAssembly()];
            });
}

var app = builder.Build();
{
    app.UseWedaCore<AppDbContext>(options =>
    {
        options.EnsureDatabaseCreated = app.Environment.IsDevelopment();
        options.SwaggerEndpointUrl = "/swagger/v1/swagger.json";
        options.SwaggerEndpointName = "Weda API V1";
        options.RoutePrefix = string.Empty;
    });

    app.Run();
}