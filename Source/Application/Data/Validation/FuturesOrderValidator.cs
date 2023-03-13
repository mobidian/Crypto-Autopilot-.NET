using Bybit.Net.Enums;

using Domain.Models;

using FluentValidation;

namespace Application.Data.Validation;

public class FuturesOrderValidator : AbstractValidator<FuturesOrder>
{
    public const string LimitOrder = nameof(LimitOrder);
    public const string MarketOrder = nameof(MarketOrder);
    public const string StatusCreated = nameof(StatusCreated);
    public const string StatusFilled = nameof(StatusFilled);
    public const string SideBuy = nameof(SideBuy);
    public const string SideSell = nameof(SideSell);
    public const string PositionLong = nameof(PositionLong);
    public const string PositionShort = nameof(PositionShort);
    public const string OrderOpenedPosition = nameof(OrderOpenedPosition);
    public const string OrderDidNotOpenPosition = nameof(OrderDidNotOpenPosition);

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
        
        this.RuleSet(StatusCreated, () => this.RuleFor(x => x.Status).Equal(OrderStatus.Created));
        this.RuleSet(StatusFilled, () => this.RuleFor(x => x.Status).Equal(OrderStatus.Filled));

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
        
        this.RuleSet(OrderOpenedPosition, () =>
        {
            this.RuleFor(x => x).Must(x =>
            {
                var marketOrder = this.Validate(x, options => options.IncludeRuleSets(MarketOrder, StatusCreated)).IsValid;
                var filledLimitOrder = this.Validate(x, options => options.IncludeRuleSets(LimitOrder, StatusFilled)).IsValid;
                return marketOrder || filledLimitOrder;
            }).WithMessage("The order must be a market order or a filled limit order in order, otherwise it cannot have opened a position");
        });
        
        this.RuleSet(OrderDidNotOpenPosition, () =>
        {
            this.RuleFor(x => x).Must(x =>
            {
                var marketOrder = this.Validate(x, options => options.IncludeRuleSets(MarketOrder)).IsValid;
                var filledLimitOrder = this.Validate(x, options => options.IncludeRuleSets(LimitOrder, StatusFilled)).IsValid;
                return !marketOrder && !filledLimitOrder;
            }).WithMessage("The order can't be a market order or a filled limit order in order, otherwise it would have opened a position");
        });
    }
}
