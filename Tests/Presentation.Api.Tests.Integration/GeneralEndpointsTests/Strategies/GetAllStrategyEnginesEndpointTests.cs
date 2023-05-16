using System.Net.Http.Json;

using CryptoAutopilot.Contracts.Responses.Strategies;

using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Strategies;

public class GetAllStrategyEnginesEndpointTests : GeneralEndpointsTestsBase
{
    [Test]
    [Ignore("Temporarily ignored because there are no defined IStrategyEngine implementations in the infrastructure layer")]
    public async Task GetAllStrategyEnginesEndpoint_ShouldReturnAllStrategyEngines_WhenStrategyEnginesExist()
    {
        // Arrange
        var engines = this.StrategyEnginesGenerator.Generate(10);
        engines.ForEach(this.StrategiesTracker.Add);
        
        // Act
        var strategiesResponse = await this.HttpClient.GetAsync("strategies");

        // Assert
        var response = await strategiesResponse.Content.ReadFromJsonAsync<GetAllStrategyEnginesResponse>();
        response!.Strategies.Should().BeEquivalentTo(engines.Select(engine => new StrategyEngineResponse
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
        var strategiesResponse = await HttpClient.GetAsync("strategies");

        // Assert
        var response = await strategiesResponse.Content.ReadFromJsonAsync<GetAllStrategyEnginesResponse>();
        response!.Strategies.Should().BeEmpty();
    }
}
