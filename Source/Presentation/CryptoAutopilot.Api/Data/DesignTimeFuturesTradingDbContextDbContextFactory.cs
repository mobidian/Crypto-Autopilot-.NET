using Infrastructure.Database;

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

        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("TradingHistoryDB-Development-Local")!);
        return new FuturesTradingDbContext(optionsBuilder.Options);
    }
}
