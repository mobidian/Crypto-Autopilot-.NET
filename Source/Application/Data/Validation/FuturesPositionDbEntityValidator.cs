using Application.Data.Entities;

using Bybit.Net.Enums;

using FluentValidation;

namespace Application.Data.Validation;

public class FuturesPositionDbEntityValidator : AbstractValidator<FuturesPositionDbEntity>
{
    public FuturesPositionDbEntityValidator()
    {
        this.RuleFor(position => position.CryptoAutopilotId).NotEqual(Guid.Empty);
        this.RuleFor(position => position.CurrencyPair).NotEqual(string.Empty);
        this.RuleFor(position => position.Margin).GreaterThanOrEqualTo(0);
        this.RuleFor(position => position.Leverage).GreaterThanOrEqualTo(0);
        this.RuleFor(position => position.Quantity).GreaterThanOrEqualTo(0);
        this.RuleFor(position => position.EntryPrice).GreaterThanOrEqualTo(0);
        this.RuleFor(position => position.ExitPrice).GreaterThanOrEqualTo(0).Unless(position => position.ExitPrice is null);
        
        this.RuleSet("CheckFkRelationships", () =>
        {
            this.RuleFor(position => position)
            .Must(position =>
            {
                if (position.FuturesOrders is null)
                    return false;

                return position.FuturesOrders.Any(o => o.Type == OrderType.Market || (o.Type == OrderType.Limit && o.Status == OrderStatus.Filled));
            })
            .WithMessage("A position cannot exist in the database without an opening futures order pointing to it.");
        });
    }
}
