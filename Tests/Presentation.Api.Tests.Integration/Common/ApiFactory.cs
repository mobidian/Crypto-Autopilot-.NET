using CryptoAutopilot.Api;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Presentation.Api.Tests.Integration.Common;

[Collection(nameof(DatabaseFixture))]
public class ApiFactory : WebApplicationFactory<IApiMarker>
{
    private readonly string ConnectionString;

    public ApiFactory(DatabaseFixture databaseFixture)
    {
        this.ConnectionString = databaseFixture.ConnectionString;
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureAppConfiguration((ctx, _) => ctx.Configuration["ConnectionStrings:TradingHistoryDB"] = this.ConnectionString);
    }
}
