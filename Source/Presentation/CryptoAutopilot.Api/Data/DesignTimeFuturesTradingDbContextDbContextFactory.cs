using Infrastructure.Database.Contexts;
using Infrastructure.Services.General;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CryptoAutopilot.Api.Data;

public class DesignTimeFuturesTradingDbContextDbContextFactory : IDesignTimeDbContextFactory<FuturesTradingDbContext>
{
    public FuturesTradingDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<Program>()
            .Build();

        return new FuturesTradingDbContext(configuration.GetConnectionString("OrderHistoryDB")!, new DateTimeProvider());
    }
}
