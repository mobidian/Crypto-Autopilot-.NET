namespace Domain.Models.Signals;

/// <summary>
/// <para>Abstract class that serves as a base for different types of signal information.</para>
/// <para>It provides a mechanism to handle different types of signal information in a uniform way.</para>
/// </summary>
public abstract class SignalInfo
{
    /// <summary>
    /// Gets the identifier that represents the type of the signal information.
    /// </summary>
    public abstract string TypeIdentifier { get; }

    /// <summary>
    /// Creates a deep copy of the current <see cref="SignalInfo"/> instance.
    /// </summary>
    /// <returns></returns>
    public abstract SignalInfo DeepClone();
}
