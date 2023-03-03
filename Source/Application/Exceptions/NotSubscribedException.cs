namespace Application.Exceptions;

public class NotSubscribedException : Exception
{
    public NotSubscribedException() { }

    public NotSubscribedException(string message) : base(message) { }

    public NotSubscribedException(string message, Exception innerException) : base(message, innerException) { }
}
