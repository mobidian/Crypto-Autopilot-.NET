using Presentation.Api.Contracts.Responses.Strategies;
using System.Net.Http.Json;

using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests;

public class GetAllStrategyEnginesEndpointTests : GeneralEndpointsTestsBase
{
    [Test]
    public async Task GetAllStrategyEnginesEndpoint_ShouldReturnAllStrategyEngines_WhenStrategyEnginesExist()
    {
        // Arrange
        var engines = this.StrategyEnginesGenerator.Generate(10);
        engines.ForEach(this.StrategiesTracker.Add);

        // Act
        var strategiesResponse = await this.HttpClient.GetAsync("strategies");

        // Assert
        var response = await strategiesResponse.Content.ReadFromJsonAsync<GetAllStrategyEnginesResponse>();
        response!.Strategies.Should().BeEquivalentTo(engines.Select(engine => new GetStrategyEngineResponse
        {
            Guid = engine.Guid,
            StartedStrategyTypeName = engine.GetType().Name,
            IsRunning = engine.IsRunning(),
        }));
    }

    [Test]
    public async Task GetAllStrategyEnginesEndpoint_ShouldReturnEmptyEnumerable_WhenNoStrategyEnginesExist()
    {
        // Act
        var strategiesResponse = await this.HttpClient.GetAsync("strategies");

        // Assert
        var response = await strategiesResponse.Content.ReadFromJsonAsync<GetAllStrategyEnginesResponse>();
        response!.Strategies.Should().BeEmpty();
    }
}
