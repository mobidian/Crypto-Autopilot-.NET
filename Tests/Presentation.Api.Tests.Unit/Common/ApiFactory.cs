using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using Presentation.Api.Services.Interfaces;

namespace Presentation.Api.Tests.Unit.Common;

public class ApiFactory : WebApplicationFactory<IApiMarker>
{
    protected readonly IStrategiesTracker StrategiesTracker;
    public ApiFactory(IStrategiesTracker strategiesTracker)
    {
        this.StrategiesTracker = strategiesTracker;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IStrategiesTracker>();
            services.AddSingleton<IStrategiesTracker>(this.StrategiesTracker);
        });
    }
}
