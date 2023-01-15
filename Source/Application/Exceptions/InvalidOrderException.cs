namespace Application.Exceptions;

public class InvalidOrderException : Exception
{
    public InvalidOrderException() { }

    public InvalidOrderException(string message) : base(message) { }

    public InvalidOrderException(string message, Exception innerException) : base(message, innerException) { }
}
