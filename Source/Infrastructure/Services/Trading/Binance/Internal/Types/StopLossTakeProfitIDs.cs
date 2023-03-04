namespace Infrastructure.Services.Trading.Binance.Internal.Types;

/// <summary>
/// Encapsulates an array of 2 IDs for stop loss and take profit orders
/// </summary>
internal class StopLossTakeProfitIdPair
{
    /// <summary>
    /// Gets the array containing the stop loss and take profit order IDs
    /// </summary>
    public long[] Values { get; } = { 0, 0 };

    /// <summary>
    /// Gets or sets the stop loss order ID
    /// </summary>    
    public long StopLoss
    {
        get => this.Values[0];
        set => this.Values[0] = value;
    }

    /// <summary>
    /// Gets or sets the take profit order ID
    /// </summary>
    public long TakeProfit
    {
        get => this.Values[1];
        set => this.Values[1] = value;
    }

    public bool HasBoth() => this.Values.All(id => id > 0);
}
