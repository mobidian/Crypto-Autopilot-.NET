using Application.Data.Entities;

using Binance.Net.Enums;

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

        builder.Property(x => x.OrderSide)
               .HasConversion(@enum => @enum.ToString(), @string => Enum.Parse<OrderSide>(@string))
               .HasMaxLength(8)
               .HasColumnName("Order Side");

        builder.Property(x => x.OrderType)
               .HasConversion(@enum => @enum.ToString(), @string => Enum.Parse<FuturesOrderType>(@string))
               .HasMaxLength(32)
               .HasColumnName("Order Type");

        builder.Property(x => x.OrderWorkingType)
               .HasConversion(@enum => @enum.ToString(), @string => Enum.Parse<WorkingType>(@string))
               .HasMaxLength(16)
               .HasColumnName("Order Working Type");
        
        builder.Property(x => x.Price).HasPrecision(18, 4);
        builder.Property(x => x.AvgPrice).HasPrecision(18, 4);
        builder.Property(x => x.StopPrice).HasPrecision(18, 4);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        
        builder.Property(x => x.TimeInForce)
               .HasConversion(@enum => @enum.ToString(), @string => Enum.Parse<TimeInForce>(@string))
               .HasMaxLength(32)
               .HasColumnName("Time in force");

        builder.Property(x => x.OrderStatus)
               .HasConversion(@enum => @enum.ToString(), @string => Enum.Parse<OrderStatus>(@string))
               .HasMaxLength(16)
               .HasColumnName("Order Status");


        builder.HasIndex(x => x.BinanceID).IsUnique();
    }
}
