using System.Net.Http.Json;

using CryptoAutopilot.Api.Contracts.Responses.Data;

using Domain.Models;

using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Data;

[Ignore("Endpoints functionality has been moved to Azure Functions\nTests are no longer applicable as they test the old endpoint implementation")]
public class GetAllCandlesticksEndpointTests : GeneralEndpointsTestsBase
{
    [Test]
    public async Task GetAllCandlesticksEndpoint_ShouldReturnAllCandlesticks_WhenCandlesticksAreInTheDatabase()
    {
        // Arrange
        var candlesticks = this.CandlestickGenerator.GenerateBetween(10, 20);

        foreach (var candlestick in candlesticks)
            await this.FuturesTradesDBService.AddCandlestickAsync(candlestick);

        // Act
        var candlesticksResponse = await this.HttpClient.GetAsync("candlesticks");

        // Assert
        var response = await candlesticksResponse.Content.ReadFromJsonAsync<GetAllCandlesticksResponse>();
        response!.Candlesticks.Should().BeEquivalentTo(candlesticks);
    }

    [Test]
    public async Task GetAllCandlesticksEndpoint_ShouldReturnEmptyEnumerable_WhenNoCandlesticksAreInTheDatabase()
    {
        // Act
        var candlesticksResponse = await this.HttpClient.GetAsync("candlesticks");

        // Assert
        var response = await candlesticksResponse.Content.ReadFromJsonAsync<GetAllCandlesticksResponse>();
        response!.Candlesticks.Should().BeEquivalentTo(Enumerable.Empty<Candlestick>());
    }
}
