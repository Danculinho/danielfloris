using Danielfloris.Audit;
using Microsoft.AspNetCore.Http;

namespace Danielfloris.Middleware;

public sealed class HttpCorrelationContext : ICorrelationContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCorrelationContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string CorrelationId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var correlationId) == true)
            {
                return correlationId?.ToString() ?? string.Empty;
            }

            return context?.TraceIdentifier ?? string.Empty;
        }
    }
}
