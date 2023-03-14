using Domain.Models;

using FluentValidation;

using static Domain.Validation.FuturesOrderValidator;

namespace Domain.Validation;

public class FuturesOrdersConsistencyValidator : AbstractValidator<IEnumerable<FuturesOrder>>
{
    private static readonly FuturesOrderValidator OrderValidator = new();


    public const string NoOrderOpensPosition = nameof(NoOrderOpensPosition);
    public const string AllOrdersOpenPosition = nameof(AllOrdersOpenPosition);

    public FuturesOrdersConsistencyValidator()
    {
        this.RuleFor(orders => orders).NotNull().NotEmpty();

        // All orders must be valid
        this.RuleFor(orders => orders).Must(orders => orders.All(order => OrderValidator.Validate(order, options => options.IncludeRulesNotInRuleSet()).IsValid)).WithMessage("All orders must be valid");

        // All orders must have the same position side
        this.RuleFor(orders => orders).Must(orders =>
        {
            var positionSide = orders.First().PositionSide;
            return orders.All(x => x.PositionSide == positionSide);
        }).WithMessage("All orders must have the same position side");

        // All orders must have the same currency pair
        this.RuleFor(orders => orders).Must(orders =>
        {
            var currencyPair = orders.First().CurrencyPair;
            return orders.All(x => x.CurrencyPair == currencyPair);
        }).WithMessage("All orders must have the same currency pair");

        this.RuleSet(NoOrderOpensPosition, () =>
        {
            this.RuleFor(orders => orders)
            .Must(orders => orders.All(DidNotOpenPosition))
            .WithMessage("No order should have opened a position");
        });

        this.RuleSet(AllOrdersOpenPosition, () =>
        {
            this.RuleFor(orders => orders)
            .Must(orders => orders.All(OpenedPosition))
            .WithMessage("All orders must have opened a position");
        });
    }



    private static bool OpenedPosition(FuturesOrder futuresOrder)
    {
        return OrderValidator.Validate(futuresOrder, x => x.IncludeRuleSets(OrderOpenedPosition)).IsValid;
    }
    private static bool DidNotOpenPosition(FuturesOrder futuresOrder)
    {
        return OrderValidator.Validate(futuresOrder, x => x.IncludeRuleSets(OrderDidNotOpenPosition)).IsValid;
    }
}
