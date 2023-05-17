using Domain.Converters;
using Domain.Models.Signals;

namespace Domain.Exceptions;

/// <summary>
/// Exception that is thrown when a <see cref="SignalInfo.TypeIdentifier"/> associated with a <see cref="SignalInfo"/> instance is not recognized by the <see cref="JsonSignalnformationConverter"/> used during JSON conversion.
/// </summary>
public class SignalTypeNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignalTypeNotFoundException"/> class.
    /// </summary>
    public SignalTypeNotFoundException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalTypeNotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SignalTypeNotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalTypeNotFoundException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public SignalTypeNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
