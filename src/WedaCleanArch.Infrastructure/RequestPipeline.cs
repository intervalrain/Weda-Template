using WedaCleanArch.Infrastructure.Common.Middleware;

using Microsoft.AspNetCore.Builder;

namespace WedaCleanArch.Infrastructure;

public static class RequestPipeline
{
    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseMiddleware<EventualConsistencyMiddleware>();
        return app;
    }
}