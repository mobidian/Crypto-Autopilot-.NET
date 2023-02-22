using System.Net.Http.Json;

using Binance.Net.Objects.Models.Futures;

using CryptoAutopilot.Api.Contracts.Responses.Data;

using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Data;

[Ignore("Endpoints functionality has been moved to Azure Functions\nTests are no longer applicable as they test the old endpoint implementation")]
public class GetAllFuturesOrdersEndpointTests : GeneralEndpointsTestsBase
{
    [Test]
    public async Task GetAllFuturesOrdersEndpoint_ShouldReturnAllFurutresOrders_WhenFurutresOrdersAreInTheDatabase()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresOrders = this.FuturesOrderGenerator.Clone().RuleFor(x => x.Symbol, candlestick.CurrencyPair.Name).GenerateBetween(10, 20);

        foreach (var order in futuresOrders)
            await this.FuturesTradesDBService.AddFuturesOrdersAsync(candlestick, order);

        // Act
        var futuresOrdersResponse = await this.HttpClient.GetAsync("futuresorders");

        // Assert
        var response = await futuresOrdersResponse.Content.ReadFromJsonAsync<GetAllFuturesOrdersResponse>();
        response!.FuturesOrders.Should().BeEquivalentTo(futuresOrders);
    }

    [Test]
    public async Task GetAllFuturesOrdersEndpoint_ShouldReturnEmptyEnumerable_WhenNoFurutresOrdersAreInTheDatabase()
    {
        // Act
        var candlesticksResponse = await this.HttpClient.GetAsync("futuresorders");

        // Assert
        var response = await candlesticksResponse.Content.ReadFromJsonAsync<GetAllFuturesOrdersResponse>();
        response!.FuturesOrders.Should().BeEquivalentTo(Enumerable.Empty<BinanceFuturesOrder>());
    }
}
