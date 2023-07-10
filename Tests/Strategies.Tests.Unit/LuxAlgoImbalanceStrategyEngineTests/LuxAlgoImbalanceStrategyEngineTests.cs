using Bogus;

using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;

using Domain.Models.Common;

using NSubstitute;

using Strategies.LuxAlgoImbalance.Enums;
using Strategies.LuxAlgoImbalance.Models;
using Strategies.Tests.Unit.LuxAlgoImbalanceStrategyEngineTests.AbstractBase;

using Xunit;

namespace Strategies.Tests.Unit.LuxAlgoImbalanceStrategyEngineTests;

public class LuxAlgoImbalanceStrategyEngineTests : LuxAlgoImbalanceStrategyEngineTestsBase
{
    [Fact]
    public async Task TakeActionAsync_ShouldPlaceLimitOrder_WhenSignalingFairValueGap()
    {
        // Arrange
        var fakeKlines = this.BybitKlinesFaker.Generate(100);
        var luxAlgoFVG = new LuxAlgoFVG
        {
            Side = new Faker().PickRandom<FvgSide>(),
            Bottom = 1800,
            Top = 1900
        };

        var orderSide = luxAlgoFVG.Side == FvgSide.Bullish ? OrderSide.Buy : OrderSide.Sell;
        var margin = 1000;
        var limitPrice = luxAlgoFVG.Middle;
        var stoploss = orderSide == OrderSide.Buy ? luxAlgoFVG.Bottom : luxAlgoFVG.Top;

        this.MarketDataProvider.GetCompletedCandlesticksAsync(this.CurrencyPair.Name, this.Timeframe).Returns(fakeKlines);
        this.FuturesAccount.GetAssetBalanceAsync(this.CurrencyPair.Name).Returns(new BybitBalance { AvailableBalance = margin });
        this.FvgFinder.FindLast(Arg.Any<IEnumerable<Candlestick>>()).Returns(luxAlgoFVG);

        _ = Task.Run(this.SUT.StartTradingAsync);
        await Task.Delay(50);


        // Act
        this.SUT.SignalFairValueGap();
        await Task.Delay(50);


        // Assert
        await this.TradingService.Received(1).PlaceLimitOrderAsync(orderSide, limitPrice, margin * 0.99m, stoploss);
    }
}
