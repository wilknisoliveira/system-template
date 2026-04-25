using Microsoft.Extensions.Logging;

namespace Shared.Utils;

public class ApplicationLogger<T>(ILogger<T> logger)
{
    public void LogInformation(string userId, string operation, string message)
    {
        logger.LogInformation("UserId: {UserId} | Operation: {Operation} | Message: {Message}", userId, operation, message);
    }

    public void LogError(Exception exception, string userId, string operation, string message)
    {
        logger.LogError(exception, "UserId: {UserId} | Operation: {Operation} | Message: {Message}", userId, operation, message);
    }
}
