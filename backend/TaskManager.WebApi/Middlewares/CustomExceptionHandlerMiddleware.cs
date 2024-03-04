using TaskManager.WebApi.Exceptions;

namespace TaskManager.WebApi.Middlewares;

public class CustomExceptionHandlerMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(exception, context);
        }
    }

    public async Task HandleExceptionAsync(Exception exception, HttpContext context)
    {
        int status = 500;
        IEnumerable<string> errors = ["Internal server error"];

        switch (exception)
        {
            case ApiException apiException:
            {
                status = apiException.Status;
                errors = apiException.Errors.Select(e => e.Message);
                break;
            }
        }

        context.Response.StatusCode = status;

        await context.Response.WriteAsJsonAsync(new
        {
            status,
            errors
        });
    }
}

public static class CustomExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
    }
}