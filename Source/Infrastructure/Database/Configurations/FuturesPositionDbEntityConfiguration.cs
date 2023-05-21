using Application.Data.Entities.Orders;

using Bybit.Net.Enums;

using Infrastructure.Database.ValueConverters.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class FuturesPositionDbEntityConfiguration : AbstractEntityTypeConfiguration<FuturesPositionDbEntity>
{
    private const string TableName = "FuturesPositions";

    public override void Configure(EntityTypeBuilder<FuturesPositionDbEntity> builder)
    {
        builder.Property(x => x.CurrencyPair).HasColumnName("Currency Pair");

        builder.Property(x => x.Side)
               .HasConversion<PositionSideConverter>()
               .HasMaxLength(8);
        builder.ToTable(TableName, t => t.HasCheckConstraint("CK_PositionSide", GetCheckSql<PositionSide>("[Side]")));

        builder.Property(x => x.Margin).HasPrecision(18, 4);
        builder.Property(x => x.Leverage).HasPrecision(18, 4);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.EntryPrice).HasPrecision(18, 4);
        builder.Property(x => x.StopLoss).HasPrecision(18, 4);
        builder.Property(x => x.TakeProfit).HasPrecision(18, 4);
        builder.Property(x => x.ExitPrice).HasPrecision(18, 4);


        builder.HasIndex(x => x.CryptoAutopilotId).IsUnique();


        builder.ToTable(TableName, tableBuilder => tableBuilder.IsTemporal());
    }
}
