using Bybit.Net.Enums;

using Domain.Commands.Orders;

using FluentValidation;

namespace Domain.Validation.Orders;

public class CreateLimitOrderCommandValidator : AbstractValidator<CreateLimitOrderCommand>
{
    private static readonly FuturesOrderValidator OrderValidator = new();

    public CreateLimitOrderCommandValidator()
    {
        this.RuleFor(command => command.LimitOrder).NotNull();
        this.RuleFor(command => command.LimitOrder).SetValidator(OrderValidator);

        this.RuleFor(command => command.LimitOrder.Type).Equal(OrderType.Limit);
        this.RuleFor(command => command.LimitOrder.Status).NotEqual(OrderStatus.Filled);
    }
}
