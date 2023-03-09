using Bybit.Net.Enums;

using Domain.Models;

using FluentValidation;

namespace Infrastructure.Validators;

public class FuturesOrderValidator : AbstractValidator<FuturesOrder>
{
    public FuturesOrderValidator()
    {
        this.RuleFor(x => x.BybitID).NotEqual(Guid.Empty);
        this.RuleFor(x => x.CurrencyPair).NotEqual(String.Empty);
        this.RuleFor(x => x.UpdateTime).GreaterThanOrEqualTo(x => x.CreateTime);
        this.RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        this.RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        this.RuleFor(x => x.StopLoss).GreaterThanOrEqualTo(0);
        this.RuleFor(x => x.TakeProfit).GreaterThanOrEqualTo(0);

        this.RuleSet("Type Limit", () => this.RuleFor(x => x.Type).Equal(OrderType.Limit));
        this.RuleSet("Type Market", () => this.RuleFor(x => x.Type).Equal(OrderType.Market));
        
        this.RuleSet("Status Created", () => this.RuleFor(x => x.Status).Equals(OrderStatus.Created));

        this.RuleSet("Side Buy", () =>
        {
            this.RuleFor(x => x.Type).Equal(OrderType.Market);
            this.RuleFor(x => x.Side).Equal(OrderSide.Buy);
            this.RuleFor(x => x.StopLoss).LessThan(x => x.Price);
            this.RuleFor(x => x.TakeProfit).GreaterThan(x => x.Price);
            this.RuleFor(x => x.TimeInForce).Equal(TimeInForce.ImmediateOrCancel);
        });
        
        this.RuleSet("Side Sell", () =>
        {
            this.RuleFor(x => x.Type).Equal(OrderType.Market);
            this.RuleFor(x => x.Side).Equal(OrderSide.Sell);
            this.RuleFor(x => x.StopLoss).GreaterThan(x => x.Price);
            this.RuleFor(x => x.TakeProfit).LessThan(x => x.Price);
            this.RuleFor(x => x.TimeInForce).Equal(TimeInForce.ImmediateOrCancel);
        });
        
        this.RuleSet("Position Long", () => this.RuleFor(x => x.PositionSide).Equal(PositionSide.Buy));
        this.RuleSet("Position Short", () => this.RuleFor(x => x.PositionSide).Equal(PositionSide.Sell));
    }
}
