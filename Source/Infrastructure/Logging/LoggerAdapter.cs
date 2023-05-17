using Application.Interfaces.Logging;

using Microsoft.Extensions.Logging;

namespace Infrastructure.Logging;

public class LoggerAdapter<TType> : ILoggerAdapter<TType>
{
    private readonly ILogger<TType> Logger;
    public LoggerAdapter(ILogger<TType> logger) => this.Logger = logger;

    public void Log(LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args) => this.Logger.Log(logLevel, eventId, exception, message, args);
    public void Log(LogLevel logLevel, Exception? exception, string? message, params object?[] args) => this.Logger.Log(logLevel, exception, message, args);
    public void Log(LogLevel logLevel, EventId eventId, string? message, params object?[] args) => this.Logger.Log(logLevel, eventId, message, args);
    public void Log(LogLevel logLevel, string? message, params object?[] args) => this.Logger.Log(logLevel, message, args);

    public void LogTrace(EventId eventId, Exception? exception, string? message, params object?[] args) => this.Logger.LogTrace(eventId, exception, message, args);
    public void LogTrace(EventId eventId, string? message, params object?[] args) => this.Logger.LogTrace(eventId, message, args);
    public void LogTrace(Exception? exception, string? message, params object?[] args) => this.Logger.LogTrace(exception, message, args);
    public void LogTrace(string? message, params object?[] args) => this.Logger.LogTrace(message, args);

    public void LogDebug(EventId eventId, Exception? exception, string? message, params object?[] args) => this.Logger.LogDebug(eventId, exception, message, args);
    public void LogDebug(EventId eventId, string? message, params object?[] args) => this.Logger.LogDebug(eventId, message, args);
    public void LogDebug(Exception? exception, string? message, params object?[] args) => this.Logger.LogDebug(exception, message, args);
    public void LogDebug(string? message, params object?[] args) => this.Logger.LogDebug(message, args);

    public void LogInformation(EventId eventId, Exception? exception, string? message, params object?[] args) => this.Logger.LogInformation(eventId, exception, message, args);
    public void LogInformation(EventId eventId, string? message, params object?[] args) => this.Logger.LogInformation(eventId, message, args);
    public void LogInformation(Exception? exception, string? message, params object?[] args) => this.Logger.LogInformation(exception, message, args);
    public void LogInformation(string? message, params object?[] args) => this.Logger.LogInformation(message, args);

    public void LogWarning(EventId eventId, Exception? exception, string? message, params object?[] args) => this.Logger.LogWarning(eventId, exception, message, args);
    public void LogWarning(EventId eventId, string? message, params object?[] args) => this.Logger.LogWarning(eventId, message, args);
    public void LogWarning(Exception? exception, string? message, params object?[] args) => this.Logger.LogWarning(exception, message, args);
    public void LogWarning(string? message, params object?[] args) => this.Logger.LogWarning(message, args);

    public void LogError(EventId eventId, Exception? exception, string? message, params object?[] args) => this.Logger.LogError(eventId, exception, message, args);
    public void LogError(EventId eventId, string? message, params object?[] args) => this.Logger.LogError(eventId, message, args);
    public void LogError(Exception? exception, string? message, params object?[] args) => this.Logger.LogError(exception, message, args);
    public void LogError(string? message, params object?[] args) => this.Logger.LogError(message, args);

    public void LogCritical(EventId eventId, Exception? exception, string? message, params object?[] args) => this.Logger.LogCritical(eventId, exception, message, args);
    public void LogCritical(EventId eventId, string? message, params object?[] args) => this.Logger.LogCritical(eventId, message, args);
    public void LogCritical(Exception? exception, string? message, params object?[] args) => this.Logger.LogCritical(exception, message, args);
    public void LogCritical(string? message, params object?[] args) => this.Logger.LogCritical(message, args);
}
