using Microsoft.Extensions.Logging;

namespace Application.Interfaces.Logging;

public interface ILoggerAdapter<TType>
{
    public void LogTrace(EventId eventId, Exception? exception, string? message, params object?[] args);

    public void LogDebug(EventId eventId, Exception? exception, string? message, params object?[] args);

    public void LogInformation(string? message, params object?[] args);

    public void LogWarning(EventId eventId, Exception? exception, string? message, params object?[] args);

    public void LogError(Exception? exception, string? message, params object?[] args);

    public void LogCritical(EventId eventId, Exception? exception, string? message, params object?[] args);
}
