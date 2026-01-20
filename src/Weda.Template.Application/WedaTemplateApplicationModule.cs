using Weda.Template.Application.Common.Behaviors;

using FluentValidation;

using Mediator;

using Microsoft.Extensions.DependencyInjection;

namespace Weda.Template.Application;

public static class WedaTemplateApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddValidatorsFromAssemblyContaining(typeof(WedaTemplateApplicationModule));
        return services;
    }
}
