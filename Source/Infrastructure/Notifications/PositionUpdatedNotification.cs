using Application.Interfaces.Services;

using Domain.Models;

using MediatR;

namespace Infrastructure.Notifications;

public class PositionUpdatedNotification : INotification
{
    public required Guid PositionCryptoAutopilotId { get; init; }
    public required FuturesPosition UpdatedPosition { get; init; }
}

public class PositionTradingStopModifiedNotificationHandler : AbstractNotificationHandler<PositionUpdatedNotification>
{
    public PositionTradingStopModifiedNotificationHandler(IFuturesTradesDBService dbService) : base(dbService) { }

    public override async Task Handle(PositionUpdatedNotification notification, CancellationToken cancellationToken)
    {
        await this.DbService.UpdateFuturesPositionAsync(notification.PositionCryptoAutopilotId, notification.UpdatedPosition);
    }
}
