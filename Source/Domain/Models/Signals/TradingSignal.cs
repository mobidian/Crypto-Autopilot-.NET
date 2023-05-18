using Domain.Models.Common;

namespace Domain.Models.Signals;

/// <summary>
/// Represents a trading signal for market transactions.
/// </summary>
public class TradingSignal
{
    /// <summary>
    /// Gets the CryptoAutopilotId of the trading signal.
    /// </summary>
    public required Guid CryptoAutopilotId { get; init; }

    /// <summary>
    /// Gets the source of the trading signal.
    /// This could be a specific trading algorithm, indicator or a third-party source.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Gets the currency pair for the trading signal.
    /// </summary>
    public required CurrencyPair CurrencyPair { get; init; }

    /// <summary>
    /// Gets the time the trading signal was generated.
    /// </summary>
    public required DateTime Time { get; init; }

    /// <summary>
    /// Gets the information of the signal. The information is subject to polymorphism as it varies based on signal type.
    /// </summary>
    public required SignalInfo Info { get; init; }
}
