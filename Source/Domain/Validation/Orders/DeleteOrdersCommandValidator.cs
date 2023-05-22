using Domain.Commands.Orders;

using FluentValidation;

namespace Domain.Validation.Orders;

public class DeleteOrdersCommandValidator : AbstractValidator<DeleteOrdersCommand>
{
    public DeleteOrdersCommandValidator()
    {
        this.RuleFor(x => x.BybitIds).NotNull().NotEmpty();
        this.RuleFor(x => x.BybitIds).Must(bybitIds => bybitIds.All(x => x != Guid.Empty));
    }
}
