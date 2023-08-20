namespace CryptoAutopilot.Api.Endpoints.Internal.Automation.General;

/// <summary>
/// An interface whose methods will be called during runtime for any type that implements it by methods from <see cref="EndpointExtensions"/>
/// </summary>
internal interface IEndpoints
{
    /// <summary>
    /// This method is called at runtime during endpoint mapping by <see cref="EndpointExtensions.MapEndpoints{TMarker}(IApplicationBuilder)"/>
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to add the endpoints to</param>
    public static abstract void MapEndpoints(IEndpointRouteBuilder app);
}
