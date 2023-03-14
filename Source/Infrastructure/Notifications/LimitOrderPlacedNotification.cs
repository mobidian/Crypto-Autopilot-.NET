using Application.Interfaces.Services;

using Domain.Models;

using MediatR;

namespace Infrastructure.Notifications;

public class LimitOrderPlacedNotification : INotification
{
    public required FuturesOrder LimitOrder { get; init; }
}

public class LimitOrderPlacedNotificationHandler : AbstractNotificationHandler<LimitOrderPlacedNotification>
{
    public LimitOrderPlacedNotificationHandler(IFuturesTradesDBService dbService) : base(dbService) { }

    public override async Task Handle(LimitOrderPlacedNotification notification, CancellationToken cancellationToken)
    {
        await this.DbService.AddFuturesOrderAsync(notification.LimitOrder);
    }
}
