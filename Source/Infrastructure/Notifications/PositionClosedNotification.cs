using Application.Interfaces.Services.Trading;

using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using MediatR;

namespace Infrastructure.Notifications;

public record PositionClosedNotification : INotification
{
    public Guid Guid { get; } = Guid.NewGuid();
    public Candlestick Candlestick { get; }
    public BinanceFuturesOrder ClosingOrder { get; }

    public PositionClosedNotification(Candlestick candlestick, BinanceFuturesOrder closingOrder)
    {
        this.Candlestick = candlestick;
        this.ClosingOrder = closingOrder;
    }
}

public class PositionClosedNotificationHandler : INotificationHandler<PositionClosedNotification>
{
    private readonly IFuturesTradesDBService FuturesTradesDBService;
    public PositionClosedNotificationHandler(IFuturesTradesDBService futuresTradesDBService)
    {
        this.FuturesTradesDBService = futuresTradesDBService;
    }

    public async Task Handle(PositionClosedNotification notification, CancellationToken cancellationToken)
    {
        await this.FuturesTradesDBService.AddFuturesOrderAsync(notification.ClosingOrder, notification.Candlestick);
    }
}