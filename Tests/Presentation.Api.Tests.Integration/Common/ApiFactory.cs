using Application.Interfaces.Services.General;

using Infrastructure.Database.Contexts;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Presentation.Api.Tests.Integration.Common;

public class ApiFactory : WebApplicationFactory<IApiMarker>
{
    private readonly string ConnectionString;

    public ApiFactory(string connectionString)
    {
        this.ConnectionString = connectionString;
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<FuturesTradingDbContext>();
            services.AddSingleton<FuturesTradingDbContext>(services => new FuturesTradingDbContext(this.ConnectionString, services.GetRequiredService<IDateTimeProvider>()));
        });
    }
}
