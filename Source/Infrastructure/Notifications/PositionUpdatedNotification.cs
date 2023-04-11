using Application.Interfaces.Services;

using Domain.Models;

using MediatR;

namespace Infrastructure.Notifications;

public class PositionUpdatedNotification : INotification
{
    public required Guid PositionCryptoAutopilotId { get; init; }
    public required FuturesPosition UpdatedPosition { get; init; }
    public FuturesOrder? EntryOrder { get; init; }
}

public class PositionTradingStopModifiedNotificationHandler : AbstractNotificationHandler<PositionUpdatedNotification>
{
    public PositionTradingStopModifiedNotificationHandler(IFuturesTradesDBService dbService) : base(dbService) { }

    public override async Task Handle(PositionUpdatedNotification notification, CancellationToken cancellationToken)
    {
        if (notification.EntryOrder is not null)
            await this.DbService.AddFuturesOrderAsync(notification.EntryOrder, notification.PositionCryptoAutopilotId);

        await this.DbService.UpdateFuturesPositionAsync(notification.PositionCryptoAutopilotId, notification.UpdatedPosition);
    }
}
