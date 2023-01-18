using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using MediatR;

namespace Infrastructure.Strategies.SimpleStrategy.Notifications;

public record PositionOpenedNotification : INotification
{
    public Guid Guid { get; } = Guid.NewGuid();
    public Candlestick Candlestick { get; }
    public FuturesPosition FuturesPosition { get; }
    
    public PositionOpenedNotification(Candlestick candlestick, FuturesPosition futuresPosition)
    {
        this.Candlestick = candlestick;
        this.FuturesPosition = futuresPosition;
    }
}

public class PositionOpenedNotificationHandler : INotificationHandler<PositionOpenedNotification>
{
    private readonly IFuturesTradesDBService FuturesTradesDBService;
    public PositionOpenedNotificationHandler(IFuturesTradesDBService futuresTradesDBService)
    {
        this.FuturesTradesDBService = futuresTradesDBService;
    }
    
    public async Task Handle(PositionOpenedNotification notification, CancellationToken cancellationToken)
    {
        IEnumerable<BinanceFuturesOrder> futuresOrders = new[]
        {
            notification.FuturesPosition.EntryOrder!,
            notification.FuturesPosition.StopLossOrder!,
            notification.FuturesPosition.TakeProfitOrder!,
        }
        .Where(x => x is not null)
        .Where(x => x.Id != 0);
        
        foreach(var futuresOrder in futuresOrders)
        {
            await this.FuturesTradesDBService.AddFuturesOrderAsync(futuresOrder, notification.Candlestick);
        }
    }
}
