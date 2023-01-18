namespace Presentation.Api.Endpoints.Internal;

/// <summary>
/// IEndpoints is an interface that provides a way to organize services and endpoints for a specific module in the application.
/// </summary>
internal interface IEndpoints
{
    /// <summary>
    /// AddServices is a method that is called during service registration to register any necessary services for the specific module.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configuration">The IConfiguration used to configure the services.</param>
    public static abstract void AddServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// MapEndpoints is a method that is called during endpoint mapping to configure the specific endpoints for the specific module.
    /// </summary>
    /// <param name="app">The IEndpointRouteBuilder to add the endpoints to.</param>
    public static abstract void MapEndpoints(IEndpointRouteBuilder app);
}
