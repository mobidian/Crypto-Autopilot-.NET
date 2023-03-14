using Application.Interfaces.Services;

using Domain.Models;

using MediatR;

namespace Infrastructure.Notifications;

public class PositionOpenedNotification : INotification
{
    public required FuturesPosition Position { get; init; }
    public required IEnumerable<FuturesOrder> FuturesOrders { get; init; }
}

public class PositionNotificationHandler : AbstractNotificationHandler<PositionOpenedNotification>
{
    public PositionNotificationHandler(IFuturesTradesDBService dbService) : base(dbService) { }
    
    public override async Task Handle(PositionOpenedNotification notification, CancellationToken cancellationToken)
    {
        await this.DbService.AddFuturesPositionAsync(notification.Position, notification.FuturesOrders);
    }
}
