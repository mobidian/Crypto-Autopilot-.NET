using Bybit.Net.Enums;

using Domain.Commands.Positions;

using FluentValidation;

namespace Domain.Validation.Positions;

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    private static readonly FuturesOrderValidator OrderValidator = new();
    private static readonly FuturesPositionValidator PositionValidator = new();

    public CreatePositionCommandValidator()
    {
        this.RuleFor(command => command.FuturesOrders).Must(orders => orders.All(x => OrderValidator.Validate(x).IsValid));
        this.RuleFor(command => command.Position).SetValidator(PositionValidator);
        
        this.RuleFor(command => command.Position).NotNull();
        
        this.RuleFor(command => command)
            .Must(command => command.FuturesOrders.All(order => order.PositionSide == command.Position.Side))
            .WithMessage("The position side must match the position side of the related orders.");
        
        this.RuleFor(command => command)
            .Must(command => command.FuturesOrders.All(order => order.Type == OrderType.Market || (order.Type == OrderType.Limit && order.Status == OrderStatus.Filled)))
            .WithMessage("A limit order which has not been filled must not point to a position.");
    }
}
