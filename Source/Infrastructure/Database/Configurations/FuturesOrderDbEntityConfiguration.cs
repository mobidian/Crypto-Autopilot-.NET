using Infrastructure.Database.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class FuturesOrderDbEntityConfiguration : IEntityTypeConfiguration<FuturesOrderDbEntity>
{
    public void Configure(EntityTypeBuilder<FuturesOrderDbEntity> builder)
    {
        builder.Property(x => x.RecordCreatedDate).HasMaxLength(50).HasColumnName("Record Created Date");
        builder.Property(x => x.RecordModifiedDate).HasMaxLength(50).HasColumnName("Record Modified Date");


        builder.Property(x => x.CandlestickId).HasColumnName("Candlestick Id");
        builder.Property(x => x.BinanceID).HasColumnName("Binance ID");
        builder.Property(x => x.OrderSide).HasMaxLength(8).HasColumnName("Order Side");
        builder.Property(x => x.OrderType).HasMaxLength(32).HasColumnName("Order Type");
        
        builder.Property(x => x.Price).HasPrecision(18, 4);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);

        builder.HasIndex(x => x.BinanceID).IsUnique();
    }
}
