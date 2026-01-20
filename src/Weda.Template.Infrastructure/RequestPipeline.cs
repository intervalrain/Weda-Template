using Microsoft.AspNetCore.Builder;
using Weda.Template.Ddd.Infrastructure.Middleware;
using Weda.Template.Infrastructure.Common;

namespace Weda.Template.Infrastructure;

public static class RequestPipeline
{
    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseMiddleware<EventualConsistencyMiddleware<AppDbContext>>();
        return app;
    }
}