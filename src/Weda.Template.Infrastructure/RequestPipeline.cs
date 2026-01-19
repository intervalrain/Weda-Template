using Weda.Template.Infrastructure.Common.Middleware;

using Microsoft.AspNetCore.Builder;

namespace Weda.Template.Infrastructure;

public static class RequestPipeline
{
    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseMiddleware<EventualConsistencyMiddleware>();
        return app;
    }
}