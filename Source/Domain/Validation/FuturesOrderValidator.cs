using Bybit.Net.Enums;

using Domain.Models.Futures;

using FluentValidation;

namespace Domain.Validation;

public class FuturesOrderValidator : AbstractValidator<FuturesOrder>
{
    public FuturesOrderValidator()
    {
        this.RuleFor(order => order.BybitID).NotEqual(Guid.Empty);
        this.RuleFor(order => order.CurrencyPair).NotEqual(string.Empty);
        this.RuleFor(order => order.UpdateTime).GreaterThanOrEqualTo(order => order.CreateTime);
        this.RuleFor(order => order.Price).GreaterThanOrEqualTo(0);
        this.RuleFor(order => order.Quantity).GreaterThanOrEqualTo(0);
        this.RuleFor(order => order.StopLoss).GreaterThanOrEqualTo(0).Unless(order => order.StopLoss is null);
        this.RuleFor(order => order.TakeProfit).GreaterThanOrEqualTo(0).Unless(order => order.TakeProfit is null);

        this.RuleFor(order => order.TimeInForce).Equal(TimeInForce.GoodTillCanceled).When(order => order.Type == OrderType.Limit);
        this.RuleFor(order => order.TimeInForce).Equal(TimeInForce.ImmediateOrCancel).When(order => order.Type == OrderType.Market);
    }
}
