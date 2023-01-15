namespace Infrastructure.Database.Entities.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime RecordCreatedDate { get; set; }
    
    public DateTime RecordModifiedDate { get; set; }
}
