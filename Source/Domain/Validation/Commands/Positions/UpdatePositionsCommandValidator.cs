using Domain.Commands.Positions;

using FluentValidation;

namespace Domain.Validation.Commands.Positions;

public class UpdatePositionsCommandValidator : AbstractValidator<UpdatePositionsCommand>
{
    private static readonly UpdatePositionCommandValidator UpdatePositionCommandValidator = new();

    public UpdatePositionsCommandValidator()
    {
        this.RuleFor(command => command.Commands).NotNull();

        this.RuleForEach(command => command.Commands).NotNull();
        this.RuleForEach(command => command.Commands).SetValidator(UpdatePositionCommandValidator);
    }
}
