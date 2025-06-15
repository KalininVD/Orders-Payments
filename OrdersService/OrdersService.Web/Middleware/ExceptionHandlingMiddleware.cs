using System.Net;
using System.Text.Json;

namespace OrdersService.Web.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception has occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var errorMessage = "An internal server error has occurred";

        switch (exception)
        {
            case FluentValidation.ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;

                var errors = validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                var validationErrorResponse = JsonSerializer.Serialize(new { errors });
                await response.WriteAsync(validationErrorResponse);

                return;

            case InvalidOperationException when exception.Message.Contains("already exists"):
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorMessage = exception.Message;
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorMessage = exception.Message;
                break;
        }

        var result = JsonSerializer.Serialize(new { error = errorMessage });

        await response.WriteAsync(result);
    }
}