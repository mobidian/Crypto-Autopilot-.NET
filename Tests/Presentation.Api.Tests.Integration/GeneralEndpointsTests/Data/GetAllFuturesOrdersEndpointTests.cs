using System.Net.Http.Json;

using Bogus;

using CryptoAutopilot.Contracts.Responses.Data.Trading.Orders;

using Domain.Models.Futures;

using FluentAssertions;

using Presentation.Api.Tests.Integration.Common;
using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

using Xunit;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Data;

public class GetAllFuturesOrdersEndpointTests : GeneralEndpointsTestsBase
{
    public GetAllFuturesOrdersEndpointTests(ApiFactory apiFactory, DatabaseFixture databaseFixture) : base(apiFactory, databaseFixture)
    {
    }


    [Fact(Skip = "Endpoints functionality has been moved to Azure Functions\nTests are no longer applicable as they test the old endpoint implementation")]
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

    [Fact(Skip = "Endpoints functionality has been moved to Azure Functions\nTests are no longer applicable as they test the old endpoint implementation")]
    public async Task GetAllFuturesOrdersEndpoint_ShouldReturnEmptyEnumerable_WhenNoFurutresOrdersAreInTheDatabase()
    {
        // Act
        var candlesticksResponse = await this.HttpClient.GetAsync("futuresorders");

        // Assert
        var response = await candlesticksResponse.Content.ReadFromJsonAsync<GetAllFuturesOrdersResponse>();
        response!.FuturesOrders.Should().BeEquivalentTo(Enumerable.Empty<FuturesOrder>());
    }
}
