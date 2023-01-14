using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

namespace Domain.Models;

public class FuturesPosition
{
    public CurrencyPair CurrencyPair { get; } = default!;

    public DateTime CreateTime => this.EntryOrder.CreateTime;

    public PositionSide Side => this.EntryOrder.Side == OrderSide.Buy ? PositionSide.Long : PositionSide.Short;

    public decimal Leverage { get; init; }
    public decimal Margin { get; init; }

    protected BinanceFuturesOrder[] OrdersBatch = new BinanceFuturesOrder[3] { null!, null!, null! };
    public BinanceFuturesOrder EntryOrder
    {
        get => this.OrdersBatch[0];
        init
        {
            this.ValidateGivenEntryOrder(value);
            this.OrdersBatch[0] = value;
        }
    }
    public BinanceFuturesOrder? StopLossOrder
    {
        get => this.OrdersBatch[1];
        set
        {
            if (value is null)
            {
                this.OrdersBatch[1] = value!;
                return;
            }

            this.ValidateGivenSLorTP(value, nameof(this.StopLossOrder));
            this.OrdersBatch[1] = value;
        }
    }
    public BinanceFuturesOrder? TakeProfitOrder
    {
        get => this.OrdersBatch[2];
        set
        {
            if (value is null)
            {
                this.OrdersBatch[2] = value!;
                return;
            }

            this.ValidateGivenSLorTP(value, nameof(this.TakeProfitOrder));
            this.OrdersBatch[2] = value;
        }
    }

    private void ValidateGivenEntryOrder(BinanceFuturesOrder value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), $"The {nameof(this.EntryOrder)} can't be initialised with a NULL value");

        if (value.Symbol != this.CurrencyPair)
            throw new ArgumentException($"The {nameof(this.EntryOrder)} property was given a value with a diffrent {typeof(CurrencyPair).Name}", nameof(value));
        
        if (value.Type != FuturesOrderType.Market)
            throw new ArgumentException($"The {nameof(this.EntryOrder)} property was given a {value.Type} order instead of a market order", nameof(value));
    }
    private void ValidateGivenSLorTP(BinanceFuturesOrder value, string paramName)
    {
        if (value.Symbol != this.CurrencyPair)
            throw new ArgumentException($"The {paramName} property was given a value with a diffrent {typeof(CurrencyPair).Name}", nameof(value));

        if (value.Type == FuturesOrderType.Market)
            throw new ArgumentException($"The {paramName} property was given a market order", nameof(value));
    }


    public IEnumerable<long> GetOrdersIDs()
    {
        foreach (BinanceFuturesOrder item in this.OrdersBatch)
            yield return item.Id;
    }

    #region Position prices of interest getters
    public decimal EntryPrice => this.EntryOrder.AvgPrice;
    public decimal? StopLossPrice => this.StopLossOrder?.StopPrice;
    public decimal? TakeProfitPrice => this.TakeProfitOrder?.StopPrice;
    #endregion

    #region Constructors
    public FuturesPosition(CurrencyPair CurrencyPair) => this.CurrencyPair = CurrencyPair;
    #endregion
}
