using Application.Interfaces.Services.Trading;

using Infrastructure.Database.Contexts;
using Infrastructure.Services.Trading;

using Presentation.Api.Endpoints.Internal;

namespace Presentation.Api.Endpoints;

public class GeneralEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        
    }
    
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/candlesticks", async (IFuturesTradesDBService DBService) => Results.Ok(await DBService.GetAllCandlesticksAsync()));

        app.MapGet("/futuresorders", async (IFuturesTradesDBService DBService) => Results.Ok(await DBService.GetAllFuturesOrdersAsync()));
    }
}
