using Domain.Commands.Positions;
using Domain.Validation.Models.Futures;

using FluentValidation;

namespace Domain.Validation.Commands.Positions;

public class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    private static readonly FuturesOrderValidator OrderValidator = new();
    private static readonly FuturesPositionValidator PositionValidator = new();

    public UpdatePositionCommandValidator()
    {
        this.RuleFor(command => command.UpdatedPosition).NotNull();
        this.RuleForEach(command => command.NewFuturesOrders).NotNull();

        this.RuleForEach(command => command.NewFuturesOrders).SetValidator(OrderValidator);
        this.RuleFor(command => command.UpdatedPosition).SetValidator(PositionValidator);


        this.RuleFor(command => command)
            .Must(command => command.NewFuturesOrders.All(order => order.PositionSide == command.UpdatedPosition.Side))
            .WithMessage("The position side must match the position side of the related orders.");
    }
}
