using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Weda.Core.Application.Behaviors;

namespace Weda.Template.Application;

public static class WedaTemplateApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
