using System.Net.Http.Json;

using CryptoAutopilot.Api.Contracts.Responses.Data;

using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Data;

[Ignore("Endpoints functionality has been moved to Azure Functions\nTests are no longer applicable as they test the old endpoint implementation")]
public class GetCandlesticksByCurrencyPairEndpointTests : GeneralEndpointsTestsBase
{
    [Test]
    public async Task GetCandlesticksByCurrencyPairEndpoint_ShouldReturnCandlesticksWithCurrencyPair_WhenCandlesticksWithCurrencyPairAreInTheDatabase()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();
        var matchingCandlesticks = this.CandlestickGenerator.Clone().RuleFor(c => c.CurrencyPair, currencyPair).GenerateBetween(10, 20);
        var randomCandlesticks = this.CandlestickGenerator.Clone().RuleFor(c => c.CurrencyPair, f => GetRandomCurrencyPairExcept(f, currencyPair)).GenerateBetween(100, 300);

        foreach (var matchingCandlestick in matchingCandlesticks)
            await this.FuturesTradesDBService.AddCandlestickAsync(matchingCandlestick);

        foreach (var randomCandlestick in randomCandlesticks)
            await this.FuturesTradesDBService.AddCandlestickAsync(randomCandlestick);


        // Act
        var candlesticksResponse = await this.HttpClient.GetAsync($"candlesticks?currencyPair={currencyPair}");


        // Assert
        var response = await candlesticksResponse.Content.ReadFromJsonAsync<GetCandlesticksByCurrencyPairResponse>();
        response!.CurrencyPair.Should().Be(currencyPair.Name);
        response!.Candlesticks.Should().BeEquivalentTo(matchingCandlesticks);
    }

    [Test]
    public async Task GetCandlesticksByCurrencyPairEndpoint_ShouldReturnEmptyEnumerable_WhenNoCandlesticksWithCurrencyPairAreInTheDatabase()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();
        var randomCandlesticks = this.CandlestickGenerator.Clone().RuleFor(c => c.CurrencyPair, f => GetRandomCurrencyPairExcept(f, currencyPair)).GenerateBetween(100, 300);

        foreach (var randomCandlestick in randomCandlesticks)
            await this.FuturesTradesDBService.AddCandlestickAsync(randomCandlestick);


        // Act
        var candlesticksResponse = await this.HttpClient.GetAsync($"candlesticks?currencyPair={currencyPair}");


        // Assert
        var response = await candlesticksResponse.Content.ReadFromJsonAsync<GetCandlesticksByCurrencyPairResponse>();
        response!.CurrencyPair.Should().Be(currencyPair.Name);
        response!.Candlesticks.Should().BeEmpty();
    }
}
