using System.Net.Http.Json;

using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using Presentation.Api.Contracts.Responses.Data;
using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Data;

public class GetFuturesOrdersByCurrencyPairEndpointTests : GeneralEndpointsTestsBase
{
    [Test]
    public async Task GetFuturesOrdersByCurrencyPairEndpoint_ShouldReturnFuturesOrdersWithCurrencyPair_WhenFuturesOrdersWithCurrencyPairAreInTheDatabase()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();

        var matchingCandlestick = this.CandlestickGenerator.Clone().RuleFor(c => c.CurrencyPair, currencyPair).Generate();
        var matchingFuturesOrders = this.FuturesOrderGenerator.Clone().RuleFor(o => o.Symbol, matchingCandlestick.CurrencyPair.Name).GenerateBetween(10, 20);
        await this.AddCandlestickAndFuturesOrdersInTheDatabaseAsync(matchingCandlestick, matchingFuturesOrders);

        for (var i = 0; i < 10; i++)
        {
            var randomCandlestick = this.CandlestickGenerator.Clone().RuleFor(c => c.CurrencyPair, f => GetRandomCurrencyPairExcept(f, currencyPair)).Generate();
            var randomFuturesOrders = this.FuturesOrderGenerator.Clone().RuleFor(o => o.Symbol, randomCandlestick.CurrencyPair.Name).GenerateBetween(10, 20);
            await this.AddCandlestickAndFuturesOrdersInTheDatabaseAsync(randomCandlestick, randomFuturesOrders);
        }

        // Act
        var futuresOrdersResponse = await this.HttpClient.GetAsync($"futuresorders?currencyPair={currencyPair}");

        // Assert
        var response = await futuresOrdersResponse.Content.ReadFromJsonAsync<GetFuturesOrdersByCurrencyPairResponse>();
        response!.CurrencyPair.Should().Be(currencyPair.Name);
        response!.FuturesOrders.Should().BeEquivalentTo(matchingFuturesOrders);
    }

    [Test]
    public async Task GetFuturesOrdersByCurrencyPairEndpoint_ShouldReturnEmptyEnumerable_WhenNoFuturesOrdersWithCurrencyPairAreInTheDatabase()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();

        for (var i = 0; i < 10; i++)
        {
            var randomCandlestick = this.CandlestickGenerator.Clone().RuleFor(c => c.CurrencyPair, f => GetRandomCurrencyPairExcept(f, currencyPair)).Generate();
            var randomFuturesOrders = this.FuturesOrderGenerator.Clone().RuleFor(o => o.Symbol, randomCandlestick.CurrencyPair.Name).GenerateBetween(10, 20);

            await this.AddCandlestickAndFuturesOrdersInTheDatabaseAsync(randomCandlestick, randomFuturesOrders);
        }

        // Act
        var futuresOrdersResponse = await this.HttpClient.GetAsync($"futuresorders?currencyPair={currencyPair}");

        // Assert
        var response = await futuresOrdersResponse.Content.ReadFromJsonAsync<GetFuturesOrdersByCurrencyPairResponse>();
        response!.CurrencyPair.Should().Be(currencyPair.Name);
        response!.FuturesOrders.Should().BeEmpty();
    }


    private async Task AddCandlestickAndFuturesOrdersInTheDatabaseAsync(Candlestick randomCandlestick, List<BinanceFuturesOrder> randomFuturesOrders)
    {
        foreach (var randomFuturesOrder in randomFuturesOrders)
            await this.FuturesTradesDBService.AddFuturesOrdersAsync(randomCandlestick, randomFuturesOrder);
    }
}
