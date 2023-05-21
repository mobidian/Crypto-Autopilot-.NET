using Application.Data.Entities.Signals;

using FluentValidation;

namespace Application.Data.Validation;

public class TradingSignalDbEntityValidator : AbstractValidator<TradingSignalDbEntity>
{
    public TradingSignalDbEntityValidator()
    {
        this.RuleFor(signal => signal.CryptoAutopilotId).NotEqual(Guid.Empty);
        this.RuleFor(signal => signal.Source).NotEmpty();
        this.RuleFor(signal => signal.CurrencyPair).NotEmpty();
    }
}
