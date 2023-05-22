using System.Net;
using System.Net.Http.Json;

using CryptoAutopilot.Contracts.Responses.Strategies;

using FluentAssertions;

using Presentation.Api.Tests.Integration.Common;
using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

using Xunit;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Strategies;

public class GetStrategyEngineByGuidEndpointTests : GeneralEndpointsTestsBase
{
    public GetStrategyEngineByGuidEndpointTests(ApiFactory apiFactory, DatabaseFixture databaseFixture) : base(apiFactory, databaseFixture)
    {
    }


    [Fact(Skip = "Temporarily ignored because there are no defined IStrategyEngine implementations in the infrastructure layer")]
    public async Task GetStrategyEngineByGuidEndpoint_ShouldReturnStrategyEngine_WhenStrategyEngineWithSpecifiedGuidExists()
    {
        // Arrange
        var i = 3;
        var engines = this.StrategyEnginesGenerator.Generate(10);
        engines.ForEach(this.StrategiesTracker.Add);

        // Act
        var strategyResponse = await this.HttpClient.GetAsync($"strategies?guid={engines[i].Guid}");

        // Assert
        var response = await strategyResponse.Content.ReadFromJsonAsync<StrategyEngineResponse>();
        response!.Guid.Should().Be(engines[i].Guid);
        response.StartedStrategyTypeName.Should().Be(engines[i].GetType().Name);
        response.IsRunning.Should().Be(engines[i].IsRunning());
    }

    [Fact]
    public async Task GetStrategyEngineByGuidEndpoint_ShouldReturnNotFound_WhenNoStrategyEngineWithSpecifiedGuidExists()
    {
        // Act
        var strategyResponse = await this.HttpClient.GetAsync($"strategies/{Guid.NewGuid()}");

        // Assert
        strategyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
