using Application.Interfaces.Services;

using MediatR;

namespace Infrastructure.Notifications;

public class CancelledLimitOrderNotification : INotification
{
    public required Guid BybitId { get; init; }
}

public class CancelledLimitOrderNotificationHandler : AbstractNotificationHandler<CancelledLimitOrderNotification>
{
    public CancelledLimitOrderNotificationHandler(IFuturesTradesDBService dbService) : base(dbService) { }

    public override async Task Handle(CancelledLimitOrderNotification notification, CancellationToken cancellationToken)
    {
        await this.DbService.DeleteFuturesOrderAsync(notification.BybitId);
    }
}
