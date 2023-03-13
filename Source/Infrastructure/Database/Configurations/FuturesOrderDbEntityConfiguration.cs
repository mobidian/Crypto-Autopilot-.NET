using Application.Data.Entities;

using Bybit.Net.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class FuturesOrderDbEntityConfiguration : IEntityTypeConfiguration<FuturesOrderDbEntity>
{
    private const string TableName = "FuturesOrders";

    public void Configure(EntityTypeBuilder<FuturesOrderDbEntity> builder)
    {
        builder.Property(x => x.BybitID).HasColumnName("Unique ID");

        builder.Property(x => x.Side)
               .HasConversion(@enum => @enum == OrderSide.Buy ? "Buy" : "Sell", @string => @string == "Buy" ? OrderSide.Buy : OrderSide.Sell)
               .HasMaxLength(8)
               .HasColumnName("Order Side");
        builder.ToTable(TableName, t => t.HasCheckConstraint("CK_OrderSide", $"[Order Side] IN ({string.Join(", ", Enum.GetValues<OrderSide>().Select(value => $"'{value}'"))})"));
        
        builder.Property(x => x.PositionSide)
               .HasConversion(@enum => @enum.ToString(), @string => Enum.Parse<PositionSide>(@string))
               .HasMaxLength(8)
               .HasColumnName("Position Side");
        builder.ToTable(TableName, t => t.HasCheckConstraint("CK_OrderPositionSide", $"[Position Side] IN ({string.Join(", ", Enum.GetValues<PositionSide>().Select(value => $"'{value}'"))})"));
        
        builder.Property(x => x.Type)
               .HasConversion(@enum => @enum.ToString(), @string => Enum.Parse<OrderType>(@string))
               .HasMaxLength(32)
               .HasColumnName("Order Type");
       builder.ToTable(TableName, t => t.HasCheckConstraint("CK_OrderType", $"[Order Type] IN ({string.Join(", ", Enum.GetValues<OrderType>().Select(value => $"'{value}'"))})"));

        builder.Property(x => x.Price).HasPrecision(18, 4);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.StopLoss).HasPrecision(18, 4);
        builder.Property(x => x.TakeProfit).HasPrecision(18, 4);
        
        builder.Property(x => x.TimeInForce)
               .HasConversion(@enum => @enum.ToString(), @string => Enum.Parse<TimeInForce>(@string))
               .HasMaxLength(32)
               .HasColumnName("Time in force");
       builder.ToTable(TableName, t => t.HasCheckConstraint("CK_TimeInForce", $"[Time in force] IN ({string.Join(", ", Enum.GetValues<TimeInForce>().Select(value => $"'{value}'"))})"));

        builder.Property(x => x.Status)
               .HasConversion(@enum => @enum.ToString(), @string => Enum.Parse<OrderStatus>(@string))
               .HasMaxLength(32)
               .HasColumnName("Order Status");
       builder.ToTable(TableName, t => t.HasCheckConstraint("CK_OrderStatus", $"[Order Status] IN ({string.Join(", ", Enum.GetValues<OrderStatus>().Select(value => $"'{value}'"))})"));


        builder.HasIndex(x => x.BybitID).IsUnique();

        
        builder.ToTable(TableName, tableBuilder => tableBuilder.IsTemporal());
    }
}
