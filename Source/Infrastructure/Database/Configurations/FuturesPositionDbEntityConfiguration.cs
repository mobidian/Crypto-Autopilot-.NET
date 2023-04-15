using Application.Data.Entities;

using Bybit.Net.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class FuturesPositionDbEntityConfiguration : IEntityTypeConfiguration<FuturesPositionDbEntity>
{
    private const string TableName = "FuturesPositions";

    public void Configure(EntityTypeBuilder<FuturesPositionDbEntity> builder)
    {
        builder.Property(x => x.CurrencyPair).HasColumnName("Currency Pair");

        builder.Property(x => x.Side)
               .HasConversion(@enum => @enum.ToString(), @string => Enum.Parse<PositionSide>(@string))
               .HasMaxLength(8);
        builder.ToTable(TableName, t => t.HasCheckConstraint("CK_PositionSide", $"[Side] IN ({string.Join(", ", Enum.GetValues<PositionSide>().Select(value => $"'{value}'"))})"));
        
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
