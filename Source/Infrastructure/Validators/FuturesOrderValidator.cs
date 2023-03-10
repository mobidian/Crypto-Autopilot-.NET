using Bybit.Net.Enums;

using Domain.Models;

using FluentValidation;

namespace Infrastructure.Validators;

public class FuturesOrderValidator : AbstractValidator<FuturesOrder>
{
    public const string LimitOrder = "LimitOrder";
    public const string MarketOrder = "MarketOrder";
    public const string StatusCreated = "StatusCreated";
    public const string SideBuy = "SideBuy";
    public const string SideSell = "SideSell";
    public const string PositionLong = "PositionLong";
    public const string PositionShort = "PositionShort";
    
    public FuturesOrderValidator()
    {
        this.RuleFor(x => x.BybitID).NotEqual(Guid.Empty);
        this.RuleFor(x => x.CurrencyPair).NotEqual(String.Empty);
        this.RuleFor(x => x.UpdateTime).GreaterThanOrEqualTo(x => x.CreateTime);
        this.RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        this.RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        this.RuleFor(x => x.StopLoss).GreaterThanOrEqualTo(0);
        this.RuleFor(x => x.TakeProfit).GreaterThanOrEqualTo(0);

        this.RuleSet(LimitOrder, () =>
        {
            this.RuleFor(x => x.Type).Equal(OrderType.Limit);
            this.RuleFor(x => x.TimeInForce).Equal(TimeInForce.GoodTillCanceled);
        });
        this.RuleSet(MarketOrder, () =>
        {
            this.RuleFor(x => x.Type).Equal(OrderType.Market);
            this.RuleFor(x => x.TimeInForce).Equal(TimeInForce.ImmediateOrCancel);
        });
        
        this.RuleSet(StatusCreated, () => this.RuleFor(x => x.Status).Equals(OrderStatus.Created));

        this.RuleSet(SideBuy, () =>
        {
            this.RuleFor(x => x.Side).Equal(OrderSide.Buy);
            this.RuleFor(x => x.StopLoss).LessThan(x => x.Price);
            this.RuleFor(x => x.TakeProfit).GreaterThan(x => x.Price);
        });
        
        this.RuleSet(SideSell, () =>
        {
            this.RuleFor(x => x.Side).Equal(OrderSide.Sell);
            this.RuleFor(x => x.StopLoss).GreaterThan(x => x.Price);
            this.RuleFor(x => x.TakeProfit).LessThan(x => x.Price);
        });
        
        this.RuleSet(PositionLong, () => this.RuleFor(x => x.PositionSide).Equal(PositionSide.Buy));
        this.RuleSet(PositionShort, () => this.RuleFor(x => x.PositionSide).Equal(PositionSide.Sell));
    }
}
