using System.Net;
using BL.Exceptions;
using BL.Models.Logic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace BL.Middleware
{
    /// <summary>
    /// Middleware for error handling
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// ExceptionHandlingMiddleware constructor
        /// </summary>
        /// <param name="next"></param>
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="httpContext"></param>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var exception = new Exception();
            var thrown = false;
            var statusCode = HttpStatusCode.InternalServerError;
            try
            {
                await _next(httpContext);
            }
            catch (ItemNotFoundException ex)
            {
                exception = ex;
                statusCode = HttpStatusCode.NotFound;
                thrown = true;
            }
            catch (UnauthorizedAccessException ex)
            {
                exception = ex;
                statusCode = HttpStatusCode.Unauthorized;
                thrown = true;
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.InternalServerError;
                exception = ex;
                thrown = true;
            }

            if (!thrown)
            {
                return;
            }
            var response = new ErrorResponse(exception);
            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.ContentType = Application.Json;
            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }

    /// <summary>
    /// Extension for Exception handling middleware
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Use ExceptionHandlingMiddleware
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
