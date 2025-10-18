using Microsoft.AspNetCore.Diagnostics;

namespace UserInterface.ExceptionHandler;

public class AppExceptionHandler(ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int statusCode, string errorMessage) = exception switch
        {
            //UserNotEnable userNotEnable => (StatusCodes.Status400BadRequest, badGatewayException.Message),
            _ => (StatusCodes.Status500InternalServerError, "Something went wrong")
        };

        logger.LogError(exception, $"{statusCode} - {exception.GetType().Name}: {errorMessage}. StackTrace: {exception.StackTrace}");

        ExceptionResponse exceptionResponse = new(statusCode, exception.GetType().Name, errorMessage);
            
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(exceptionResponse, cancellationToken);

        return true;
    }
}
