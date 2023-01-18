using Infrastructure.Services.Trading.Strategy;

using Presentation.Api.Endpoints.Internal;

namespace Presentation.Api.Endpoints;

public class MyStrategyEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<MyStrategyEngine>();
    }

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("mysignal", (MyStrategyEngine engine) =>
        {
            
        });
    }
}
