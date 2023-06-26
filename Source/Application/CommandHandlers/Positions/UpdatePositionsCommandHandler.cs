using Application.DataAccess.Services;

using Domain.Commands.Positions;

using MediatR;

namespace Application.CommandHandlers.Positions;

public class UpdatePositionsCommandHandler : IRequestHandler<UpdatePositionsCommand, Unit>
{
    private readonly IFuturesOperationsService FuturesOperationsService;
    public UpdatePositionsCommandHandler(IFuturesOperationsService futuresOperationsService) => this.FuturesOperationsService = futuresOperationsService;

    public async Task<Unit> Handle(UpdatePositionsCommand request, CancellationToken cancellationToken)
    {
        var positionsOrders = request.Commands.ToDictionary(x => x.UpdatedPosition, x => x.NewFuturesOrders);
        await this.FuturesOperationsService.UpdateFuturesPositionsAndAddTheirOrdersAsync(positionsOrders);
        return Unit.Value;
    }
}
