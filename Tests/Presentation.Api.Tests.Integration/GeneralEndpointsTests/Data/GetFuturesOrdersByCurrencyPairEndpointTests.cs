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

public class GetFuturesOrdersByCurrencyPairEndpointTests : GeneralEndpointsTestsBase
{
    public GetFuturesOrdersByCurrencyPairEndpointTests(ApiFactory apiFactory, DatabaseFixture databaseFixture) : base(apiFactory, databaseFixture)
    {
    }


    [Fact]
    public async Task GetFuturesOrdersByCurrencyPairEndpoint_ShouldReturnFuturesOrdersWithCurrencyPair_WhenFuturesOrdersWithCurrencyPairAreInTheDatabase()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();
        var futuresOrders = this.FuturesOrdersGenerator.Clone().RuleFor(o => o.CurrencyPair, currencyPair).GenerateBetween(10, 20);
        await this.ArrangeOrdersRepository.AddAsync(futuresOrders);

        for (var i = 0; i < 10; i++)
        {
            var randomCurrencyPair = this.CurrencyPairGenerator.Generate();
            var randomFuturesOrders = this.FuturesOrdersGenerator.Clone().RuleFor(o => o.CurrencyPair, randomCurrencyPair).GenerateBetween(10, 20);
            await this.ArrangeOrdersRepository.AddAsync(randomFuturesOrders);
        }

        // Act
        var futuresOrdersResponse = await this.HttpClient.GetAsync($"Data/Trading/Orders?contractName={currencyPair}");

        // Assert
        var response = await futuresOrdersResponse.Content.ReadFromJsonAsync<GetFuturesOrdersByContractNameResponse>();
        response!.ContractName.Should().Be(currencyPair.Name);
        response!.FuturesOrders.Should().BeEquivalentTo(futuresOrders.ToResponses());
    }

    [Fact]
    public async Task GetFuturesOrdersByCurrencyPairEndpoint_ShouldReturnEmptyEnumerable_WhenNoFuturesOrdersWithCurrencyPairAreInTheDatabase()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();

        for (var i = 0; i < 10; i++)
        {
            var randomCurrencyPair = this.CurrencyPairGenerator.Generate();
            var randomFuturesOrders = this.FuturesOrdersGenerator.Clone().RuleFor(o => o.CurrencyPair, randomCurrencyPair).GenerateBetween(10, 20);
            await this.ArrangeOrdersRepository.AddAsync(randomFuturesOrders);
        }

        // Act
        var futuresOrdersResponse = await this.HttpClient.GetAsync($"Data/Trading/Orders?contractName={currencyPair}");

        // Assert
        var response = await futuresOrdersResponse.Content.ReadFromJsonAsync<GetFuturesOrdersByContractNameResponse>();
        response!.ContractName.Should().Be(currencyPair.Name);
        response!.FuturesOrders.Should().BeEmpty();
    }
}
