using Application.DataAccess.Services;

using Domain.Commands.Positions;

using MediatR;

namespace Application.CommandHandlers.Positions;

public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, Unit>
{
    private readonly IFuturesOperationsService FuturesOperationsService;
    public CreatePositionCommandHandler(IFuturesOperationsService futuresOperationsService) => this.FuturesOperationsService = futuresOperationsService;

    public async Task<Unit> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        var position = request.Position;
        var orders = request.FuturesOrders;

        await this.FuturesOperationsService.AddFuturesPositionAndOrdersAsync(position, orders);
        return Unit.Value;
    }
}
