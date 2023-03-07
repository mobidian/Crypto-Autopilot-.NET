using Application.Interfaces.Services.Trading.Bybit;

using Bybit.Net.Clients;
using Bybit.Net.Enums;
using Bybit.Net.Objects;

using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;

using Domain.Models;

using Infrastructure.Services.General;
using Infrastructure.Services.Trading.Bybit;
using Infrastructure.Tests.Integration.Common;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

public abstract class BybitUsdFuturesTradingServiceTestsBase
{
    private readonly SecretsManager SecretsManager = new SecretsManager();

    protected readonly Faker faker = new Faker();

    protected readonly BybitUsdFuturesTradingService SUT;
    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("BTC", "USDT");
    protected decimal Margin = 100;
    protected readonly decimal Leverage = 10m;
    protected readonly IBybitFuturesAccountDataProvider FuturesAccount;
    protected readonly IBybitUsdFuturesMarketDataProvider MarketDataProvider;
    protected readonly IBybitUsdFuturesTradingApiClient TradingClient;

    public BybitUsdFuturesTradingServiceTestsBase()
    {
        var bybitClient = new BybitClient(new BybitClientOptions
        {
            UsdPerpetualApiOptions = new RestApiClientOptions
            {
                ApiCredentials = new ApiCredentials(this.SecretsManager.GetSecret("BybitTestnetApiCredentials:key"), this.SecretsManager.GetSecret("BybitTestnetApiCredentials:secret")),
                BaseAddress = "https://api-testnet.bybit.com"
            }
        });

        this.FuturesAccount = new BybitFuturesAccountDataProvider(bybitClient.UsdPerpetualApi.Account);
        this.TradingClient = new BybitUsdFuturesTradingApiClient(bybitClient.UsdPerpetualApi.Trading);
        this.MarketDataProvider = new BybitUsdFuturesMarketDataProvider(new DateTimeProvider(), bybitClient.UsdPerpetualApi.ExchangeData);

        this.SUT = new BybitUsdFuturesTradingService(this.CurrencyPair, this.Leverage, this.FuturesAccount, this.MarketDataProvider, this.TradingClient);
    }


    [TearDown]
    public async Task TearDown()
    {
        if (this.SUT.LongPosition is not null)
            await this.SUT.ClosePositionAsync(PositionSide.Buy);

        if (this.SUT.ShortPosition is not null)
            await this.SUT.ClosePositionAsync(PositionSide.Sell);

        if (this.SUT.BuyLimitOrder is not null)
            await this.SUT.CancelLimitOrderAsync(OrderSide.Buy);

        if (this.SUT.SellLimitOrder is not null)
            await this.SUT.CancelLimitOrderAsync(OrderSide.Sell);
    }
}
