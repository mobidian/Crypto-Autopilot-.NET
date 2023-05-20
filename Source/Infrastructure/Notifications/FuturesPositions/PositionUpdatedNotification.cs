using Application.Interfaces.Services.DataAccess.Repositories;

using Domain.Models.Orders;

using MediatR;

using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Notifications.FuturesPositions;

/// <summary>
/// Represents a notification that is published when an existing position is updated.
/// </summary>
public class PositionUpdatedNotification : INotification
{
    /// <summary>
    /// Gets or initializes the updated FuturesPosition.
    /// </summary>
    public required FuturesPosition UpdatedPosition { get; init; }

    /// <summary>
    /// Gets or initializes the collection of related FuturesOrders for the updated position.
    /// </summary>
    public IEnumerable<FuturesOrder> FuturesOrders { get; init; } = Enumerable.Empty<FuturesOrder>();
}

public class PositionUpdatedNotificationHandler : INotificationHandler<PositionUpdatedNotification>
{
    private readonly IFuturesOrdersRepository OrdersRepository;
    private readonly IFuturesPositionsRepository PositionsRepository;

    public PositionUpdatedNotificationHandler(IFuturesOrdersRepository ordersRepository, IFuturesPositionsRepository positionsRepository)
    {
        this.OrdersRepository = ordersRepository;
        this.PositionsRepository = positionsRepository;
    }


    public async Task Handle(PositionUpdatedNotification notification, CancellationToken cancellationToken)
    {
        if (!notification.FuturesOrders.IsNullOrEmpty())
            await this.OrdersRepository.AddFuturesOrdersAsync(notification.FuturesOrders, notification.UpdatedPosition.CryptoAutopilotId);
        
        await this.PositionsRepository.UpdateFuturesPositionAsync(notification.UpdatedPosition.CryptoAutopilotId, notification.UpdatedPosition);
    }
}
