using Application.Data.Entities.Futures;

using Bybit.Net.Enums;

using Infrastructure.Database.ValueConverters.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class FuturesOrderDbEntityConfiguration : AbstractEntityTypeConfiguration<FuturesOrderDbEntity>
{
    private const string TableName = "FuturesOrders";

    public override void Configure(EntityTypeBuilder<FuturesOrderDbEntity> builder)
    {
        builder.Property(x => x.BybitID).HasColumnName("Unique ID");

        builder.Property(x => x.Side)
               .HasConversion<OrderSideConverter>()
               .HasMaxLength(8)
               .HasColumnName("Order Side");
        builder.ToTable(TableName, t => t.HasCheckConstraint("CK_OrderSide", GetCheckSql<OrderSide>("[Order Side]")));

        builder.Property(x => x.PositionSide)
               .HasConversion<PositionSideConverter>()
               .HasMaxLength(8)
               .HasColumnName("Position Side");
        builder.ToTable(TableName, t => t.HasCheckConstraint("CK_OrderPositionSide", GetCheckSql<PositionSide>("[Position Side]")));

        builder.Property(x => x.Type)
               .HasConversion<OrderTypeConverter>()
               .HasMaxLength(32)
               .HasColumnName("Order Type");
        builder.ToTable(TableName, t => t.HasCheckConstraint("CK_OrderType", GetCheckSql<OrderType>("[Order Type]")));

        builder.Property(x => x.Price).HasPrecision(18, 4);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.StopLoss).HasPrecision(18, 4);
        builder.Property(x => x.TakeProfit).HasPrecision(18, 4);

        builder.Property(x => x.TimeInForce)
               .HasConversion<TimeInForceConverter>()
               .HasMaxLength(32)
               .HasColumnName("Time in force");
        builder.ToTable(TableName, t => t.HasCheckConstraint("CK_TimeInForce", GetCheckSql<TimeInForce>("[Time in force]")));

        builder.Property(x => x.Status)
               .HasConversion<OrderStatusConverter>()
               .HasMaxLength(32)
               .HasColumnName("Order Status");
        builder.ToTable(TableName, t => t.HasCheckConstraint("CK_OrderStatus", GetCheckSql<OrderStatus>("[Order Status]")));


        builder.HasIndex(x => x.BybitID).IsUnique();


        builder.ToTable(TableName, tableBuilder => tableBuilder.IsTemporal());
    }
}
