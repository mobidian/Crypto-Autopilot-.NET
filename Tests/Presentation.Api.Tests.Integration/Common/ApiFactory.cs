using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Presentation.Api.Tests.Integration.Common;

public class ApiFactory : WebApplicationFactory<IApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureAppConfiguration(configuration => configuration.AddUserSecrets<ApiFactory>(optional: false, reloadOnChange: true));
    }
}
