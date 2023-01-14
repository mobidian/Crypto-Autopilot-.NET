namespace Application.Interfaces.Logging;

public interface ILoggerAdapter<TType>
{
    public void LogInformation(string? message, params object?[] args);
    
    public void LogError(Exception? exception, string? message, params object?[] args);
}
