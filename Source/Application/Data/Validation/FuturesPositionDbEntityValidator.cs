using Application.Data.Entities;

using FluentValidation;

namespace Application.Data.Validation;

public class FuturesPositionDbEntityValidator : AbstractValidator<FuturesPositionDbEntity>
{
    public FuturesPositionDbEntityValidator()
    {
        this.RuleFor(position => position.CryptoAutopilotId).NotEqual(Guid.Empty);
        this.RuleFor(position => position.CurrencyPair).NotEqual(string.Empty);
        this.RuleFor(position => position.Margin).GreaterThanOrEqualTo(0);
        this.RuleFor(position => position.Leverage).GreaterThanOrEqualTo(1);
        this.RuleFor(position => position.Quantity).GreaterThanOrEqualTo(0);
        this.RuleFor(position => position.EntryPrice).GreaterThanOrEqualTo(0);
        this.RuleFor(position => position.ExitPrice).GreaterThanOrEqualTo(0).Unless(position => position.ExitPrice is null);

        this.RuleSet("CheckFkRelationships", () =>
        {
            this.RuleFor(position => position)
                .Must(position =>
                {
                    if (position.FuturesOrders is null)
                        return true;

                    return position.FuturesOrders.All(x => x.PositionSide == position.Side);
                })
                .WithMessage("An order which points to a position must have the appropriate value for the PositionSide property.");
        });
    }
}
