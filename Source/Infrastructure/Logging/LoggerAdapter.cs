using Application.Interfaces.Logging;

using Microsoft.Extensions.Logging;

namespace Infrastructure.Logging;

public class LoggerAdapter<TType> : ILoggerAdapter<TType>
{
    private readonly ILogger<TType> Logger;

    public LoggerAdapter(ILogger<TType> logger) => this.Logger = logger;


    public void LogInformation(string? message, params object?[] args) => this.Logger.LogInformation(message, args);

    public void LogError(Exception? exception, string? message, params object?[] args) => this.Logger.LogError(exception, message, args);
}
