namespace Application.Data.Entities.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime RecordCreatedDate { get; set; }

    public DateTime RecordModifiedDate { get; set; }
}
