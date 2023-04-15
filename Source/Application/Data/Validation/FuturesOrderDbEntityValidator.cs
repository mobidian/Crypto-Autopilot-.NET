using Application.Data.Entities;
using Application.Extensions.Bybit;

using Bybit.Net.Enums;

using FluentValidation;

namespace Application.Data.Validation;

public class FuturesOrderDbEntityValidator : AbstractValidator<FuturesOrderDbEntity>
{
    public FuturesOrderDbEntityValidator()
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

        this.RuleSet("CheckFkRelationships", () =>
        {
            this.RuleFor(order => order)
                .Must(order => order.PositionId is not null && order.Side.ToPositionSide() == order.PositionSide)
                .When(order => order.Type == OrderType.Limit && order.Status == OrderStatus.Filled)
                .WithMessage("A filled limit order must point to a position with the appropriate position side.");

            this.RuleFor(order => order)
                .Must(order => order.PositionId is not null)
                .When(order => order.Type == OrderType.Market)
                .WithMessage("A market order must point to a position.");

            this.RuleFor(order => order)
                .Must(order => order.PositionId is null)
                .When(order => order.Type == OrderType.Limit && order.Status != OrderStatus.Filled)
                .WithMessage("A limit order which has not been filled must not point to a position.");
        });
    }
}
