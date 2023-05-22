using FluentValidation;

using MediatR;

namespace Domain.PipelineBehaviors;

public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> Validators;
    
    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        this.Validators = validators;
    }


    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var ctx = new ValidationContext<TRequest>(request);
        var failures = this.Validators
            .Select(x => x.Validate(ctx))
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .ToArray(); // by materializing the query the validators are only executed once, preventing duplication of error messages

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return next.Invoke();
    }
}
