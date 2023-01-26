﻿using System.Net.Http.Json;

using Binance.Net.Objects.Models.Futures;

using Presentation.Api.Contracts.Responses;
using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests;

public class GetAllFuturesOrdersEndpointTests : GeneralEndpointsTestsBase
{
    [Test]
    public async Task GetAllFuturesOrdersEndpoint_ShouldReturnAllFurutresOrders_WhenFurutresOrdersAreInTheDatabase()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresOrders = this.FuturesOrderGenerator.Clone().RuleFor(x => x.Symbol, candlestick.CurrencyPair.Name).GenerateBetween(10, 20);

        foreach (var order in futuresOrders)
            await this.FuturesTradesDBService.AddFuturesOrderAsync(candlestick, order);

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
