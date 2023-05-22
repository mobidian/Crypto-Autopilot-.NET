using Domain.Commands.Positions;

using FluentValidation;

namespace Domain.Validation.Positions;

public class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    private static readonly FuturesPositionValidator PositionValidator = new();

    public UpdatePositionCommandValidator()
    {
        this.RuleFor(command => command.UpdatedPosition).SetValidator(PositionValidator);

        this.RuleFor(command => command.UpdatedPosition).NotNull();
        
        this.RuleFor(command => command)
            .Must(command => command.NewFuturesOrders.All(order => order.PositionSide == command.UpdatedPosition.Side))
            .WithMessage("The position side must match the position side of the related orders.");
    }
}
