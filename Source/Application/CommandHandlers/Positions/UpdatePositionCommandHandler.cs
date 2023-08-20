using Application.DataAccess.Services;

using Domain.Commands.Positions;

using MediatR;

namespace Application.CommandHandlers.Positions;

public class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand, Unit>
{
    private readonly IFuturesOperationsService FuturesOperationsService;
    public UpdatePositionCommandHandler(IFuturesOperationsService futuresOperationsService) => this.FuturesOperationsService = futuresOperationsService;

    public async Task<Unit> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        var updatedPosition = request.UpdatedPosition;
        var newFuturesOrders = request.NewFuturesOrders;

        await this.FuturesOperationsService.UpdateFuturesPositionAndAddOrdersAsync(updatedPosition, newFuturesOrders);
        return Unit.Value;
    }
}
