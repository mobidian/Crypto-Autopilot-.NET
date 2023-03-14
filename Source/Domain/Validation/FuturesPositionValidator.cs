﻿using Bybit.Net.Enums;

using Domain.Models;

using FluentValidation;

namespace Domain.Validation;

public class FuturesPositionValidator : AbstractValidator<FuturesPosition>
{
    public const string Long = "Long";
    public const string Short = "Short";

    public FuturesPositionValidator()
    {
        this.RuleFor(x => x.CryptoAutopilotId).NotEqual(Guid.Empty);
        this.RuleFor(x => x.CurrencyPair).NotEqual(string.Empty);
        this.RuleFor(x => x.Margin).GreaterThanOrEqualTo(0);
        this.RuleFor(x => x.Leverage).GreaterThanOrEqualTo(0);
        this.RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        this.RuleFor(x => x.EntryPrice).GreaterThanOrEqualTo(0);
        this.RuleFor(x => x.ExitPrice).GreaterThanOrEqualTo(0).Unless(x => x.ExitPrice is null);

        this.RuleSet(Long, () => this.RuleFor(x => x.Side).Equal(PositionSide.Buy));
        this.RuleSet(Short, () => this.RuleFor(x => x.Side).Equal(PositionSide.Sell));
    }
}
