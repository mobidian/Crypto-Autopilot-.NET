using Application.Data.Entities.Futures;

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
    }
}
