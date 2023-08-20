using System.Reflection;

using CryptoAutopilot.Api.Endpoints.Internal.Automation.General;

namespace CryptoAutopilot.Api.Endpoints.Internal.Automation.General;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> and <see cref="IApplicationBuilder"/> to register services and add endpoints
/// </summary>
internal static class EndpointExtensions
{
    private static IEnumerable<Type> GetEndpointTypes(Assembly assembly) => assembly.DefinedTypes.Where(typeInfo => !typeInfo.IsAbstract && !typeInfo.IsInterface && typeof(IEndpoints).IsAssignableFrom(typeInfo));

    /// <summary>
    /// Scans everything in the specified type's assembly and finds every class that implements the <see cref="IEndpoints"/> interface and dynamically calls <see cref="IEndpoints.MapEndpoints"/>
    /// </summary>
    /// <typeparam name="TMarker">The type marker for the assembly to be scanned</typeparam>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to add the endpoints to</param>
    internal static void MapEndpoints<TMarker>(this IApplicationBuilder app)
    {
        var endpointTypes = GetEndpointTypes(typeof(TMarker).Assembly);
        foreach (var endpointType in endpointTypes)
            endpointType.GetMethod(nameof(IEndpoints.MapEndpoints))!.Invoke(null, new object[] { app });
    }
}
