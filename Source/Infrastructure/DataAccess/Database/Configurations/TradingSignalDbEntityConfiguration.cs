using Application.Data.Entities.Signals;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Database.Configurations;

public class TradingSignalDbEntityConfiguration : IEntityTypeConfiguration<TradingSignalDbEntity>
{
    private const string TableName = "TradingSignals";

    public void Configure(EntityTypeBuilder<TradingSignalDbEntity> builder)
    {
        builder.Property(x => x.CurrencyPair).HasColumnName("Currency Pair").HasMaxLength(32);


        builder.HasIndex(x => x.CryptoAutopilotId).IsUnique();
    }
}
