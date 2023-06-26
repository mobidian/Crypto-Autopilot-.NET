using System.Net.Http.Json;

using Bogus;

using CryptoAutopilot.Contracts.Responses.Data.Trading.Orders;

using Domain.Models.Common;
using Domain.Models.Futures;

using FluentAssertions;

using Presentation.Api.Tests.Integration.Common;
using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

using Xunit;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Data;

public class GetFuturesOrdersByCurrencyPairEndpointTests : GeneralEndpointsTestsBase
{
    public GetFuturesOrdersByCurrencyPairEndpointTests(ApiFactory apiFactory, DatabaseFixture databaseFixture) : base(apiFactory, databaseFixture)
    {
    }


    [Fact(Skip = "Endpoints functionality has been moved to Azure Functions\nTests are no longer applicable as they test the old endpoint implementation")]
    public async Task GetFuturesOrdersByCurrencyPairEndpoint_ShouldReturnFuturesOrdersWithCurrencyPair_WhenFuturesOrdersWithCurrencyPairAreInTheDatabase()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();

        var matchingCandlestick = this.CandlestickGenerator.Clone().RuleFor(c => c.CurrencyPair, currencyPair).Generate();
        var matchingFuturesOrders = this.FuturesOrderGenerator.Clone().RuleFor(o => o.CurrencyPair, matchingCandlestick.CurrencyPair).GenerateBetween(10, 20);
        await this.AddCandlestickAndFuturesOrdersInTheDatabaseAsync(matchingCandlestick, matchingFuturesOrders);

        for (var i = 0; i < 10; i++)
        {
            var randomCandlestick = this.CandlestickGenerator.Clone().RuleFor(c => c.CurrencyPair, f => GetRandomCurrencyPairExcept(f, currencyPair)).Generate();
            var randomFuturesOrders = this.FuturesOrderGenerator.Clone().RuleFor(o => o.CurrencyPair, randomCandlestick.CurrencyPair).GenerateBetween(10, 20);
            await this.AddCandlestickAndFuturesOrdersInTheDatabaseAsync(randomCandlestick, randomFuturesOrders);
        }

        // Act
        var futuresOrdersResponse = await this.HttpClient.GetAsync($"futuresorders?currencyPair={currencyPair}");

        // Assert
        var response = await futuresOrdersResponse.Content.ReadFromJsonAsync<GetFuturesOrdersByContractNameResponse>();
        response!.ContractName.Should().Be(currencyPair.Name);
        response!.FuturesOrders.Should().BeEquivalentTo(matchingFuturesOrders);
    }

    [Fact(Skip = "Endpoints functionality has been moved to Azure Functions\nTests are no longer applicable as they test the old endpoint implementation")]
    public async Task GetFuturesOrdersByCurrencyPairEndpoint_ShouldReturnEmptyEnumerable_WhenNoFuturesOrdersWithCurrencyPairAreInTheDatabase()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();

        for (var i = 0; i < 10; i++)
        {
            var randomCandlestick = this.CandlestickGenerator.Clone().RuleFor(c => c.CurrencyPair, f => GetRandomCurrencyPairExcept(f, currencyPair)).Generate();
            var randomFuturesOrders = this.FuturesOrderGenerator.Clone().RuleFor(o => o.CurrencyPair, randomCandlestick.CurrencyPair).GenerateBetween(10, 20);

            await this.AddCandlestickAndFuturesOrdersInTheDatabaseAsync(randomCandlestick, randomFuturesOrders);
        }

        // Act
        var futuresOrdersResponse = await this.HttpClient.GetAsync($"futuresorders?currencyPair={currencyPair}");

        // Assert
        var response = await futuresOrdersResponse.Content.ReadFromJsonAsync<GetFuturesOrdersByContractNameResponse>();
        response!.ContractName.Should().Be(currencyPair.Name);
        response!.FuturesOrders.Should().BeEmpty();
    }


    private async Task AddCandlestickAndFuturesOrdersInTheDatabaseAsync(Candlestick randomCandlestick, List<FuturesOrder> randomFuturesOrders)
    {
        foreach (var randomFuturesOrder in randomFuturesOrders)
            await this.OrdersRepository.AddAsync(randomFuturesOrder);
    }
}
