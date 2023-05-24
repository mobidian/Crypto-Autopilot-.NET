using Bybit.Net.Enums;

using Domain.Commands.Orders;
using Domain.Validation.Models.Futures;

using FluentValidation;

namespace Domain.Validation.Commands.Orders;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    private static readonly FuturesOrderValidator OrderValidator = new();

    public UpdateOrderCommandValidator()
    {
        this.RuleFor(command => command.UpdatedOrder).NotNull();
        this.RuleFor(command => command.UpdatedOrder).SetValidator(OrderValidator);
        this.RuleFor(command => command.FuturesPositionId).Must(id => id != Guid.Empty).WithMessage($"The FuturesPositionId can't be '00000000-0000-0000-0000-000000000000'");

        this.RuleFor(command => command.FuturesPositionId)
            .NotNull()
            .When(command => command.UpdatedOrder.Type == OrderType.Market || command.UpdatedOrder.Type == OrderType.Limit && command.UpdatedOrder.Status == OrderStatus.Filled)
            .WithMessage("The FuturesPositionId can't be null when the updated order is a position opening order.");

        this.RuleFor(command => command.FuturesPositionId)
            .Null()
            .When(command => command.UpdatedOrder.Type == OrderType.Limit && command.UpdatedOrder.Status == OrderStatus.Created)
            .WithMessage("The FuturesPositionId must be null when the updated order isn't a position opening order.");
    }
}
