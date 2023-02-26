using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using Domain.Extensions;

namespace Domain.Models;

public class FuturesPosition
{
    public required CurrencyPair CurrencyPair { get; init; } = default!;

    public DateTime CreateTime => this.EntryOrder.CreateTime;
    public PositionSide Side => this.EntryOrder.PositionSide;

    public decimal EntryPrice => this.EntryOrder.AvgPrice;
    public decimal? StopLossPrice => this.StopLossOrder?.StopPrice;
    public decimal? TakeProfitPrice => this.TakeProfitOrder?.StopPrice;


    public required decimal Leverage { get; init; }
    public required decimal Margin { get; init; }

    protected BinanceFuturesOrder[] OrdersBatch = new BinanceFuturesOrder[3] { null!, null!, null! };
    public required BinanceFuturesOrder EntryOrder
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
        
        var marketOrder = value.Type == FuturesOrderType.Market;
        var filledLimitOrder = value.Type == FuturesOrderType.Limit && value.Status == OrderStatus.Filled;
        if (!marketOrder && !filledLimitOrder)
            throw new ArgumentException($"The {nameof(this.EntryOrder)} property was given a {value.Status} {value.Type} order", nameof(value));

        if (value.Side.ToPositionSide() != value.PositionSide)
            throw new ArgumentException($"The {nameof(this.EntryOrder)} property was given an order with conflicting order side and position side values", nameof(value));
    }
    private void ValidateGivenSLorTP(BinanceFuturesOrder value, string paramName)
    {
        if (value.Symbol != this.CurrencyPair)
            throw new ArgumentException($"The {paramName} property was given a value with a diffrent {typeof(CurrencyPair).Name}", nameof(value));

        if (value.Type == FuturesOrderType.Market)
            throw new ArgumentException($"The {paramName} property was given a market order", nameof(value));
        
        if (value.Side.ToPositionSide() != value.PositionSide.Invert())
            throw new ArgumentException($"The {paramName} property was given an order with conflicting order side and position side values", nameof(value));
    }

    
    /// <summary>
    /// Returns the binance IDs of the open orders
    /// </summary>
    /// <returns></returns>
    public IEnumerable<long> GetOpenOrdersIDs()
    {
        foreach (BinanceFuturesOrder item in this.OrdersBatch.Where(x => x is not null))
            yield return item.Id;
    }
}
