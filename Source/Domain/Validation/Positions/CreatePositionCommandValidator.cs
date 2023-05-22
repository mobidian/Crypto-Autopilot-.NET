using Domain.Commands.Positions;

using FluentValidation;

namespace Domain.Validation.Positions;

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    private static readonly FuturesPositionValidator PositionValidator = new();

    public CreatePositionCommandValidator()
    {
        this.RuleFor(command => command.Position).SetValidator(PositionValidator);

        this.RuleFor(command => command.Position).NotNull();
        
        this.RuleFor(command => command)
            .Must(command => command.FuturesOrders.All(order => order.PositionSide == command.Position.Side))
            .WithMessage("The position side must match the position side of the related orders.");
    }
}
