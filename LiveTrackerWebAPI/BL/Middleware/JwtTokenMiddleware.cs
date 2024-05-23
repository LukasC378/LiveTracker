using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BL.Middleware;

public class JwtTokenMiddleware
{
    private readonly RequestDelegate _next;

    public JwtTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var accessToken = context.Request.Cookies["X-Access-Token"];
        if (!string.IsNullOrEmpty(accessToken))
        {
            context.Request.Headers["Authorization"] = "Bearer " + accessToken;
        }

        await _next(context);
    }
}

public static class JwtTokenMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtTokenMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtTokenMiddleware>();
    }
}