using Domain.Commands.Orders;

using FluentValidation;

namespace Domain.Validation.Orders;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    private static readonly FuturesOrderValidator OrderValidator = new();

    public UpdateOrderCommandValidator()
    {
        this.RuleFor(command => command.UpdatedOrder).SetValidator(OrderValidator);
    }
}
