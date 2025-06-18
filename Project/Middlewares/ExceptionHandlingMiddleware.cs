using System.Net;
using Project.Exceptions;

namespace Project.Middlewares;

public static class ExceptionHandlingMiddlewareExtension
{
    public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (UnauthorizedException e)
        {
            await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, e.Message);
        }
        catch (UnauthorizedAccessException e)
        {
            await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, e.Message);
        }
        catch (ArgumentException e)
        {
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, e.Message);
        }
        catch (InvalidOperationException e)
        {
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, e.Message);
        }
        catch (Exception)
        {
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        var response = new { message };
        return context.Response.WriteAsJsonAsync(response);
    }
}