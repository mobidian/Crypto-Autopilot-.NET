using Domain.Models;

using FluentValidation;

namespace Infrastructure.Validators;

public class FuturesOrdersConsistencyValidator : AbstractValidator<IEnumerable<FuturesOrder>>
{
    private static readonly FuturesOrderValidator OrderValidator = new();
    
    public FuturesOrdersConsistencyValidator()
    {
        this.RuleFor(orders => orders).NotNull().NotEmpty();
        
        // // TODO // //   
    }
}