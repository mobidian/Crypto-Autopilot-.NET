using System.Net;
using System.Net.Http.Json;

using CryptoAutopilot.Api.Contracts.Responses.Strategies;

using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Strategies;

public class GetStrategyEngineByGuidEndpointTests : GeneralEndpointsTestsBase
{
    [Test]
    [Ignore("Temporarily ignored because there are no defined IStrategyEngine implementations in the infrastructure layer")]
    public async Task GetStrategyEngineByGuidEndpoint_ShouldReturnStrategyEngine_WhenStrategyEngineWithSpecifiedGuidExists()
    {
        // Arrange
        var i = 3;
        var engines = this.StrategyEnginesGenerator.Generate(10);
        engines.ForEach(this.StrategiesTracker.Add);

        // Act
        var strategyResponse = await this.HttpClient.GetAsync($"strategies?guid={engines[i].Guid}");

        // Assert
        var response = await strategyResponse.Content.ReadFromJsonAsync<GetStrategyEngineResponse>();
        response!.Guid.Should().Be(engines[i].Guid);
        response.StartedStrategyTypeName.Should().Be(engines[i].GetType().Name);
        response.IsRunning.Should().Be(engines[i].IsRunning());
    }

    [Test]
    public async Task GetStrategyEngineByGuidEndpoint_ShouldReturnNotFound_WhenNoStrategyEngineWithSpecifiedGuidExists()
    {
        // Act
        var strategyResponse = await this.HttpClient.GetAsync($"strategies/{Guid.NewGuid()}");

        // Assert
        strategyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
