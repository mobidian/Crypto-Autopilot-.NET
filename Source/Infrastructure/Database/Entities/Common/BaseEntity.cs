namespace Infrastructure.Database.Entities.Common;

internal abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }
}
