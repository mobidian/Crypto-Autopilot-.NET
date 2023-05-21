using System.Net.Http.Json;

using CryptoAutopilot.Contracts.Responses.Data.Trading.Orders;

using Domain.Models.Futures;

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
        var futuresOrders = this.FuturesOrderGenerator.Clone().RuleFor(x => x.CurrencyPair, candlestick.CurrencyPair).GenerateBetween(10, 20);
        
        foreach (var order in futuresOrders)
            await this.OrdersRepository.AddFuturesOrderAsync(order);

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
        response!.FuturesOrders.Should().BeEquivalentTo(Enumerable.Empty<FuturesOrder>());
    }
}
