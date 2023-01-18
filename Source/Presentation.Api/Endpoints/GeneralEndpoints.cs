using Presentation.Api.Endpoints.Internal;

namespace Presentation.Api.Endpoints;

public class GeneralEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        
    }

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("get", () =>
        {

        });
    }
}
