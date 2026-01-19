using Weda.Template.Application.Common.Behaviors;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace Weda.Template.Application;

public static class WedaTemplateApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(WedaTemplateApplicationModule).Assembly);

            options.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssemblyContaining(typeof(WedaTemplateApplicationModule));
        return services;
    }
}