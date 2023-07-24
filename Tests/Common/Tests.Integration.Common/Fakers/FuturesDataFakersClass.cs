using Bogus;

using Bybit.Net.Enums;

using Domain.Models.Common;
using Domain.Models.Futures;

using Tests.Integration.Common.DataAccess.Extensions;

namespace Tests.Integration.Common.Fakers;

public abstract class FuturesDataFakersClass : TradingSignalsFakersClass
{
    private const int decimals = 4;


    protected readonly Faker Faker = new();

    protected readonly Faker<CurrencyPair> CurrencyPairGenerator = new Faker<CurrencyPair>()
        .CustomInstantiator(f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code));

    protected readonly Faker<Candlestick> CandlestickGenerator = new Faker<Candlestick>()
        .RuleFor(c => c.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(c => c.Date, f => f.Date.Recent(365))
        .RuleFor(c => c.Open, f => Math.Round(f.Random.Decimal(1000, 1500), decimals))
        .RuleFor(c => c.High, (f, c) => Math.Round(f.Random.Decimal(c.Open, c.Open + 100), decimals))
        .RuleFor(c => c.Low, (f, c) => Math.Round(f.Random.Decimal(c.Open - 100, c.Open), decimals))
        .RuleFor(c => c.Close, (f, c) => Math.Round(f.Random.Decimal(1000, 1500), decimals))
        .RuleFor(c => c.Volume, f => Math.Round(f.Random.Decimal(100000, 300000), decimals));

    protected readonly Faker<FuturesOrder> FuturesOrdersGenerator = new Faker<FuturesOrder>()
        .RuleFor(o => o.BybitID, f => Guid.NewGuid())
        .RuleFor(o => o.CurrencyPair, f => new CurrencyPair("BTC", "USDT"))
        .RuleFor(o => o.CreateTime, f => f.Date.Past(1))
        .RuleFor(o => o.UpdateTime, (f, p) => p.CreateTime.AddHours(f.Random.Int(0, 12)))
        .RuleFor(o => o.Price, f => f.Random.Decimal(5000, 15000, decimals))
        .RuleFor(o => o.Quantity, f => f.Random.Decimal(100, 300, decimals))
        .RuleFor(o => o.Status, OrderStatus.Created)
        .RuleSet(OrderType.Limit.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.Type, OrderType.Limit);
            set.RuleFor(o => o.TimeInForce, TimeInForce.GoodTillCanceled);
        })
        .RuleSet(OrderType.Market.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.Type, OrderType.Market);
            set.RuleFor(o => o.TimeInForce, TimeInForce.ImmediateOrCancel);
        })
        .RuleSet(OrderStatus.Filled.ToRuleSetName(), set => set.RuleFor(o => o.Status, OrderStatus.Filled))
        .RuleSet(OrderSide.Buy.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.Side, f => OrderSide.Buy);
            set.RuleFor(o => o.StopLoss, (f, p) => f.Random.Decimal(p.Price, p.Price - 3000, decimals));
            set.RuleFor(o => o.TakeProfit, (f, p) => f.Random.Decimal(p.Price, p.Price + 3000, decimals));
        })
        .RuleSet(OrderSide.Sell.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.Side, f => OrderSide.Sell);
            set.RuleFor(o => o.StopLoss, (f, p) => f.Random.Decimal(p.Price, p.Price + 3000, decimals));
            set.RuleFor(o => o.TakeProfit, (f, p) => f.Random.Decimal(p.Price, p.Price - 3000, decimals));
        })
        .RuleSet(PositionSide.Buy.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.PositionSide, f => PositionSide.Buy);
        })
        .RuleSet(PositionSide.Sell.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.PositionSide, f => PositionSide.Sell);
        });

    protected readonly Faker<FuturesPosition> FuturesPositionsGenerator = new Faker<FuturesPosition>()
        .RuleFor(p => p.CryptoAutopilotId, f => Guid.NewGuid())
        .RuleFor(p => p.CurrencyPair, f => new CurrencyPair("BTC", "USDT"))
        .RuleFor(p => p.Margin, f => f.Random.Decimal(1, 1000, decimals))
        .RuleFor(p => p.Leverage, f => f.Random.Decimal(1, 100, decimals))
        .RuleFor(p => p.EntryPrice, f => f.Random.Decimal(5000, 15000, decimals))
        .RuleFor(p => p.Quantity, (_, p) => Math.Round(p.Margin * p.Leverage / p.EntryPrice, decimals))
        .RuleSet(PositionSide.Buy.ToRuleSetName(), set =>
        {
            set.RuleFor(p => p.Side, PositionSide.Buy);
            set.RuleFor(p => p.ExitPrice, (f, p) => f.Random.Decimal(p.EntryPrice, p.EntryPrice + 3000, decimals));
        })
        .RuleSet(PositionSide.Sell.ToRuleSetName(), set =>
        {
            set.RuleFor(p => p.Side, PositionSide.Sell);
            set.RuleFor(p => p.ExitPrice, (f, p) => f.Random.Decimal(p.EntryPrice, p.EntryPrice - 3000, decimals));
        });
}
