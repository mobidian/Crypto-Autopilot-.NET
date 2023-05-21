using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public abstract class AbstractEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
{
    public abstract void Configure(EntityTypeBuilder<TEntity> builder);

    protected static string GetCheckSql<TEnum>(string tableName) where TEnum : struct, Enum
        => $"{tableName} IN ({string.Join(", ", Enum.GetValues<TEnum>().Select(value => $"'{value}'"))})";
}
