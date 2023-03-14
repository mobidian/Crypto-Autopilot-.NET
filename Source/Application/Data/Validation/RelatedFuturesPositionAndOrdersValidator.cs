using System.ComponentModel;

using Bybit.Net.Enums;

using Domain.Models;

using FluentValidation;

using static Application.Data.Validation.FuturesOrdersConsistencyValidator;
using static Application.Data.Validation.FuturesOrderValidator;

namespace Application.Data.Validation;

public class RelatedFuturesPositionAndOrdersValidator : AbstractValidator<(FuturesPosition Position, IEnumerable<FuturesOrder> Orders)>
{
    private static readonly FuturesPositionValidator PositionValidator = new();
    private static readonly FuturesOrdersConsistencyValidator OrdersConsistencyValidator = new();
    private static readonly FuturesOrderValidator OrderValidator = new();

    public RelatedFuturesPositionAndOrdersValidator()
    {
        this.RuleFor(x => x.Position)
            .Must(position => PositionValidator.Validate(position).IsValid)
            .WithMessage("The position must be valid");

        this.RuleFor(x => x.Orders)
            .Must(orders => OrdersConsistencyValidator.Validate(orders).IsValid)
            .WithMessage("All orders must be consistent");

        this.RuleFor(x => x.Orders)
            .Must(orders => OrdersConsistencyValidator.Validate(orders, options => options.IncludeRuleSets(AllOrdersOpenPosition)).IsValid)
            .WithMessage("All orders must have opened a position");

        this.RuleFor(x => x)
            .Must(x =>
            {
                var rule = x.Position.Side switch
                {
                    PositionSide.Buy => PositionLong,
                    PositionSide.Sell => PositionShort,
                    _ => throw new InvalidEnumArgumentException("An invalid position side has been specified", (int)x.Position.Side, typeof(PositionSide))
                };

                return x.Orders.All(order => OrderValidator.Validate(order, options => options.IncludeRuleSets(rule)).IsValid);
            })
            .WithMessage("All orders position side must match the side of the position");
    }
}
