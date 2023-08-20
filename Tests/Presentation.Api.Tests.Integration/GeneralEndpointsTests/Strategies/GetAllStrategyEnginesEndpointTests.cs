using System.Net.Http.Json;

using CryptoAutopilot.Api.Endpoints;
using CryptoAutopilot.Contracts.Responses.Strategies;

using FluentAssertions;

using Presentation.Api.Tests.Integration.Common;
using Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Strategies;

public class GetAllStrategyEnginesEndpointTests : GeneralEndpointsTestsBase
{
    public GetAllStrategyEnginesEndpointTests(ApiFactory apiFactory, DatabaseFixture databaseFixture) : base(apiFactory, databaseFixture)
    {
    }


    [Fact(Skip = "Temporarily ignored because there are no defined IStrategyEngine implementations in the infrastructure layer")]
    public async Task GetAllStrategyEnginesEndpoint_ShouldReturnAllStrategyEngines_WhenStrategyEnginesExist()
    {
        // Arrange
        var engines = this.StrategyEnginesGenerator.Generate(10);
        engines.ForEach(this.StrategiesTracker.Add);

        // Act
        var strategiesResponse = await this.HttpClient.GetAsync(ApiEndpoints.Strategies.GetAll);

        // Assert
        var response = await strategiesResponse.Content.ReadFromJsonAsync<GetAllStrategyEnginesResponse>();
        response!.Strategies.Should().BeEquivalentTo(engines.Select(engine => new StrategyEngineResponse
        {
            Guid = engine.Guid,
            StartedStrategyTypeName = engine.GetType().Name,
            IsRunning = engine.IsRunning(),
        }));
    }

    [Fact]
    public async Task GetAllStrategyEnginesEndpoint_ShouldReturnEmptyEnumerable_WhenNoStrategyEnginesExist()
    {
        // Act
        var strategiesResponse = await this.HttpClient.GetAsync(ApiEndpoints.Strategies.GetAll);

        // Assert
        var response = await strategiesResponse.Content.ReadFromJsonAsync<GetAllStrategyEnginesResponse>();
        response!.Strategies.Should().BeEmpty();
    }
}
