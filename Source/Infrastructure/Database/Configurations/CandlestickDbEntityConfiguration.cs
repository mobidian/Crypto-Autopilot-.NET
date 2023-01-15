using Application.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class CandlestickDbEntityConfiguration : IEntityTypeConfiguration<CandlestickDbEntity>
{
    public void Configure(EntityTypeBuilder<CandlestickDbEntity> builder)
    {
        builder.Property(x => x.RecordCreatedDate).HasMaxLength(50).HasColumnName("Record Created Date");
        builder.Property(x => x.RecordModifiedDate).HasMaxLength(50).HasColumnName("Record Modified Date");

        
        builder.Property(x => x.BaseCurrency).HasMaxLength(4).HasColumnName("Base Currency");
        builder.Property(x => x.QuoteCurrency).HasMaxLength(4).HasColumnName("Quote Currency");
        builder.Property(x => x.Open).HasPrecision(18, 4);
        builder.Property(x => x.High).HasPrecision(18, 4);
        builder.Property(x => x.Low).HasPrecision(18, 4);
        builder.Property(x => x.Close).HasPrecision(18, 4);
        builder.Property(x => x.Volume).HasPrecision(18, 4);

        builder.HasIndex(x => x.BaseCurrency);
        builder.HasIndex(x => x.QuoteCurrency);
        builder.HasIndex(x => new { x.BaseCurrency, x.QuoteCurrency });
        builder.HasIndex(x => new { x.BaseCurrency, x.QuoteCurrency, x.DateTime }).IsUnique();
    }
}
