namespace Application.Exceptions;

/// <summary>
/// The exception thrown when an error within the <see cref="ICfdTradingService"/> occurs
/// </summary>
public class InternalTradingServiceException : Exception
{
    public InternalTradingServiceException() { }

    public InternalTradingServiceException(string message) : base(message) { }

    public InternalTradingServiceException(string message, Exception innerException) : base(message, innerException) { }
}
