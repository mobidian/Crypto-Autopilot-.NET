using Application.Interfaces.Services;

using MediatR;

namespace Infrastructure.Notifications;

public abstract class AbstractNotificationHandler<TNotification> : INotificationHandler<TNotification> where TNotification : INotification
{
    protected readonly IFuturesTradesDBService DbService;
    protected AbstractNotificationHandler(IFuturesTradesDBService dbService) => this.DbService = dbService;

    public abstract Task Handle(TNotification notification, CancellationToken cancellationToken);
}
