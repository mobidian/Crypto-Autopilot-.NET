using Application.Interfaces.Services;

using Domain.Models;

using MediatR;

namespace Infrastructure.Notifications;

public class UpdatedLimitOrderNotification : INotification
{
    public required Guid BybitId { get; init; }
    public required FuturesOrder UpdatedLimitOrder { get; init; }
}

public class UpdatedLimitOrderNotificationHandler : AbstractNotificationHandler<UpdatedLimitOrderNotification>
{
    public UpdatedLimitOrderNotificationHandler(IFuturesTradesDBService dbService) : base(dbService) { }
    
    public override async Task Handle(UpdatedLimitOrderNotification notification, CancellationToken cancellationToken)
    {
        await this.DbService.UpdateFuturesOrderAsync(notification.BybitId, notification.UpdatedLimitOrder);
    }
}
