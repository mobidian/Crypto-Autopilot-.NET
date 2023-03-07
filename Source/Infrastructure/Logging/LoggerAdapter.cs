using Application.Interfaces.Logging;

using Microsoft.Extensions.Logging;

namespace Infrastructure.Logging;

public class LoggerAdapter<TType> : ILoggerAdapter<TType>
{
    private readonly ILogger<TType> Logger;

    public LoggerAdapter(ILogger<TType> logger) => this.Logger = logger;

    
    public void LogTrace(EventId eventId, Exception? exception, string? message, params object?[] args) => this.Logger.LogTrace(eventId, exception, message, args);
    
    public void LogDebug(EventId eventId, Exception? exception, string? message, params object?[] args) => this.Logger.LogDebug(eventId, exception, message, args);
    
    public void LogInformation(string? message, params object?[] args) => this.Logger.LogInformation(message, args);

    public void LogWarning(EventId eventId, Exception? exception, string? message, params object?[] args) => this.Logger.LogWarning(eventId, exception, message, args);

    public void LogError(Exception? exception, string? message, params object?[] args) => this.Logger.LogError(exception, message, args);

    public void LogCritical(EventId eventId, Exception? exception, string? message, params object?[] args) => this.Logger.LogCritical(eventId, exception, message, args);
}
