using System.Reflection;

using Application.Strategies;

using CryptoAutopilot.Api.Services;
using CryptoAutopilot.Api.Services.Interfaces;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CryptoAutopilot.Api.Endpoints.Strategies.Automation;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> and <see cref="IApplicationBuilder"/> to register trading strategies and add endpoints for them
/// </summary>
internal static class StrategyEndpointExtensions
{
    private static IEnumerable<Type> GetStrategyEndpointTypes(Assembly assembly) => assembly.DefinedTypes.Where(typeInfo => !typeInfo.IsAbstract && !typeInfo.IsInterface && typeInfo.GetInterface(typeof(IStrategyEndpoints<>).Name) is not null);

    /// <summary>
    /// Scans everything in the specified type's assembly and finds every class that implements the <see cref="IStrategyEndpoints{}"/> interface and dynamically calls <see cref="IStrategyEndpoints{}.AddStrategy(IServiceCollection, IConfiguration)"/> and registers <see cref="IStrategiesTracker"/> as a singleton if it hasn't been registered already
    /// </summary>
    /// <typeparam name="TMarker">The type marker for the assembly to be scanned</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the trading strategies to</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> used to configure the trading strategies</param>
    internal static void AddStrategies<TMarker>(this IServiceCollection services, IConfiguration configuration)
    {
        var endpointTypes = GetStrategyEndpointTypes(typeof(TMarker).Assembly);
        foreach (var endpointType in endpointTypes)
            endpointType.GetMethod(nameof(IStrategyEndpoints<IStrategyEngine>.AddStrategy))!.Invoke(null, new object[] { services, configuration });

        services.TryAddSingleton<IStrategiesTracker, StrategiesTracker>();
    }

    /// <summary>
    /// Scans everything in the specified type's assembly and finds every class that implements the <see cref="IStrategyEndpoints{}"/> interface and dynamically calls <see cref="IStrategyEndpoints{}.MapStrategySignalsEndpoints(IEndpointRouteBuilder)"/> and <see cref="IStrategyEndpoints{}.MapStartStopEndpoints(IEndpointRouteBuilder)"/>
    /// </summary>
    /// <typeparam name="TMarker">The type marker for the assembly to be scanned</typeparam>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to add the endpoints to</param>
    internal static void MapStrategyEndpoints<TMarker>(this IApplicationBuilder app)
    {
        var endpointTypes = GetStrategyEndpointTypes(typeof(TMarker).Assembly);
        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IStrategyEndpoints<IStrategyEngine>.MapStrategySignalsEndpoints))!.Invoke(null, new object[] { app });
            endpointType.GetInterface(typeof(IStrategyEndpoints<>).Name)!.GetMethod(nameof(IStrategyEndpoints<IStrategyEngine>.MapStartStopEndpoints))!.Invoke(null, new object[] { app });
        }
    }
}
