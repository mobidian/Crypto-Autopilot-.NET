using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Infrastructure.Database.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class CandlestickDbEntityConfiguration : IEntityTypeConfiguration<CandlestickDbEntity>
{
    public void Configure(EntityTypeBuilder<CandlestickDbEntity> builder)
    {
        builder.Property(x => x.CurrencyPair).HasMaxLength(8);
        builder.Property(x => x.Open).HasPrecision(18, 4);
        builder.Property(x => x.High).HasPrecision(18, 4);
        builder.Property(x => x.Low).HasPrecision(18, 4);
        builder.Property(x => x.Close).HasPrecision(18, 4);

        builder.HasIndex(x => x.CurrencyPair);

        builder.HasIndex(x => new { x.CurrencyPair, x.DateTime }).IsUnique();
    }
}
