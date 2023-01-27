namespace Presentation.Api.Endpoints.Internal;

internal static class EndpointExtensions
{
    private static IEnumerable<Type> GetEndpointTypesFromAssemblyContaining(Type typeMarker) => typeMarker.Assembly.DefinedTypes.Where(typeInfo => !typeInfo.IsAbstract && !typeInfo.IsInterface && typeof(IEndpoints).IsAssignableFrom(typeInfo));

    /// <summary>
    /// Scans everything in the given assembly and finds every class that implements the <see cref="IEndpoints"/> interface and dynamically calls <see cref="IEndpoints.AddServices"/>
    /// </summary>
    internal static void AddServices<TMarker>(this IServiceCollection services, IConfiguration configuration)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeof(TMarker));
        foreach (Type endpointType in endpointTypes)
            endpointType.GetMethod(nameof(IEndpoints.AddServices))!.Invoke(null, new object[] { services, configuration });
    }
    
    /// <summary>
    /// Scans everything in the given assembly and finds every class that implements the <see cref="IEndpoints"/> interface and dynamically calls <see cref="IEndpoints.MapEndpoints"/>
    /// </summary>
    internal static void MapEndpoints<TMarker>(this IApplicationBuilder app)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeof(TMarker));
        foreach (Type endpointType in endpointTypes)
            endpointType.GetMethod(nameof(IEndpoints.MapEndpoints))!.Invoke(null, new object[] { app });
    }
}
