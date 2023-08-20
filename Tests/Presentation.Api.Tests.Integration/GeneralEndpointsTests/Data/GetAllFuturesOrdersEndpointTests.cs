using System.Net.Http.Json;

using Bogus;

using CryptoAutopilot.Api.Endpoints.Extensions;
using CryptoAutopilot.Contracts.Responses.Data.Trading.Orders;

using FluentAssertions;

using Presentation.Api.Tests.Integration.Common;
using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Data;

public class GetAllFuturesOrdersEndpointTests : GeneralEndpointsTestsBase
{
    public GetAllFuturesOrdersEndpointTests(ApiFactory apiFactory, DatabaseFixture databaseFixture) : base(apiFactory, databaseFixture)
    {
    }


    [Fact]
    public async Task GetAllFuturesOrdersEndpoint_ShouldReturnAllFurutresOrders_WhenFurutresOrdersAreInTheDatabase()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresOrders = this.FuturesOrdersGenerator.Clone().RuleFor(x => x.CurrencyPair, candlestick.CurrencyPair).GenerateBetween(10, 20);
        await this.ArrangeOrdersRepository.AddAsync(futuresOrders);

        // Act
        var futuresOrdersResponse = await this.HttpClient.GetAsync("Data/Trading/Orders");

        // Assert
        var response = await futuresOrdersResponse.Content.ReadFromJsonAsync<GetAllFuturesOrdersResponse>();
        response!.FuturesOrders.Should().BeEquivalentTo(futuresOrders.ToResponses());
    }

    [Fact]
    public async Task GetAllFuturesOrdersEndpoint_ShouldReturnEmptyEnumerable_WhenNoFurutresOrdersAreInTheDatabase()
    {
        // Act
        var candlesticksResponse = await this.HttpClient.GetAsync("Data/Trading/Orders");

        // Assert
        var response = await candlesticksResponse.Content.ReadFromJsonAsync<GetAllFuturesOrdersResponse>();
        response!.FuturesOrders.Should().BeEmpty();
    }
}
