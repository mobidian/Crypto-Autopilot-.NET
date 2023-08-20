using System.Text.RegularExpressions;

using Application.Strategies;

using Microsoft.AspNetCore.Mvc;

namespace CryptoAutopilot.Api.Endpoints.Internal.Automation.Strategies;

/// <summary>
/// An interface whose methods will be called during runtime for any type that implements it by methods from <see cref="StrategyEndpointExtensions"/>
/// </summary>
/// <typeparam name="TStrategyEngine">The type of trading strategy that will be added and the endpoints will be mapped for</typeparam>
internal interface IStrategyEndpoints<TStrategyEngine> where TStrategyEngine : class, IStrategyEngine
{
    /// <summary>
    /// This method that is called at runtime during service registration by <see cref="StrategyEndpointExtensions.AddStrategies{TMarker}(IServiceCollection, IConfiguration)"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the trading strategy to</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> used to configure the trading strategy</param>
    public static abstract void AddStrategy(IServiceCollection services, IConfiguration configuration);


    /// <summary>
    /// This method is called at runtime during endpoint mapping by <see cref="StrategyEndpointExtensions.MapStrategyEndpoints{TMarker}(IApplicationBuilder)"/>
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to add the endpoints to</param>
    public static abstract void MapStrategySignalsEndpoints(IEndpointRouteBuilder app);

    /// <summary>
    /// This method is called at runtime during endpoint mapping by <see cref="StrategyEndpointExtensions.MapStrategyEndpoints{TMarker}(IApplicationBuilder)"/>
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to add the endpoints to</param>
    public static void MapStartStopEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(Regex.Replace($"Start{typeof(TStrategyEngine).Name}", @"(Engine)$", string.Empty, RegexOptions.IgnoreCase), async ([FromServices] TStrategyEngine engine, IServiceProvider services) => await engine.StartAsync(services, TimeSpan.FromSeconds(15))).WithTags(typeof(TStrategyEngine).Name);

        app.MapPost(Regex.Replace($"Stop{typeof(TStrategyEngine).Name}", @"(Engine)$", string.Empty, RegexOptions.IgnoreCase), async ([FromServices] TStrategyEngine engine, IServiceProvider services) => await engine.StopAsync(services, TimeSpan.FromSeconds(15))).WithTags(typeof(TStrategyEngine).Name);
    }
}
