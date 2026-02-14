using Danielfloris.Audit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Danielfloris.Middleware;

public static class CorrelationIdExtensions
{
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationContext, HttpCorrelationContext>();
        return services;
    }

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
