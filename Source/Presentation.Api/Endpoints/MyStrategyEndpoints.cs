using Infrastructure.Strategies.SimpleStrategy;
using Presentation.Api.Endpoints.Internal;

namespace Presentation.Api.Endpoints;

public class MyStrategyEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<SimpleStrategyEngine>();
    }

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("mysignal", (SimpleStrategyEngine engine) =>
        {
            
        });
    }
}
