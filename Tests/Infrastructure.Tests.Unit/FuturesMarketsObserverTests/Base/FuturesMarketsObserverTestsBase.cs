using Application.Interfaces.Logging;

using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Spot.Socket;

using CryptoExchange.Net.Objects;

using CryptoExchange.Net.Sockets;

using Domain.Models;

using Infrastructure.Logging;
using Infrastructure.Services.Trading;

namespace Infrastructure.Tests.Unit.FuturesMarketsObserverTests.Base;

public abstract class FuturesMarketsObserverTestsBase
{
    protected readonly Random RNG = new Random();

    protected FuturesMarketsCandlestickAwaiter SUT;
    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");
    protected readonly IBinanceSocketClientUsdFuturesStreams FuturesStreams = Substitute.For<IBinanceSocketClientUsdFuturesStreams>();
    protected readonly ILoggerAdapter<FuturesMarketsCandlestickAwaiter> Logger = Substitute.For<ILoggerAdapter<FuturesMarketsCandlestickAwaiter>>();

    public FuturesMarketsObserverTestsBase()
    {
        this.FuturesStreams
            .SubscribeToKlineUpdatesAsync(
                Arg.Any<string>(),
                Arg.Any<KlineInterval>(),
                Arg.Any<Action<DataEvent<IBinanceStreamKlineData>>>())
            .Returns(Task.FromResult(new CallResult<UpdateSubscription>(new UpdateSubscription(null!, null!))));

        this.SUT = new FuturesMarketsCandlestickAwaiter(this.CurrencyPair, KlineInterval.OneHour, this.FuturesStreams, this.Logger);
    }

    protected async Task SUT_SubscribeToKlineUpdatesAsync(DateTime KlineOpenTime, BinanceStreamKline streamKline = default!)
    {
        await this.SUT_SubscribeToKlineUpdatesAsync(CreateDataEvent(KlineOpenTime, streamKline));
    }
    protected async Task SUT_SubscribeToKlineUpdatesAsync(DataEvent<IBinanceStreamKlineData> dataEvent)
    {
        var task = Task.Run(this.SUT.SubscribeToKlineUpdatesAsync);
        this.SUT.HandleKlineUpdate(dataEvent);
        await task;
    }
    protected static DataEvent<IBinanceStreamKlineData> CreateDataEvent(DateTime KlineOpenTime, BinanceStreamKline streamKline = default!)
    {
        var dataEvent = new DataEvent<IBinanceStreamKlineData>(new BinanceStreamKlineData(), DateTime.MinValue);
        dataEvent.Data.Data = streamKline ?? new BinanceStreamKline();
        dataEvent.Data.Data.OpenTime = KlineOpenTime;
        return dataEvent;
    }
}
