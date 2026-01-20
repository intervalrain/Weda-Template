using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Weda.Template.Application.Common.Behaviors;

namespace Weda.Template.Application;

public static class WedaTemplateApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        return services;
    }
}
