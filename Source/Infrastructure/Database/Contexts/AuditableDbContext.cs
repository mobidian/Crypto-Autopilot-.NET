using Application.Data.Entities.Common;
using Application.Interfaces.Services.General;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Contexts;

public abstract class AuditableDbContext : DbContext
{
    private readonly IDateTimeProvider DateTimeProvider;
    public AuditableDbContext(IDateTimeProvider dateTimeProvider) => DateTimeProvider = dateTimeProvider;

    
    // base.SaveChanges() => base.SaveChanges(acceptAllChangesOnSuccess: true);
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.OnBeforeSavedChanges();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    // base.SaveChangesAsync(cancellationToken) => base.SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken); 
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.OnBeforeSavedChanges();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    
    private void OnBeforeSavedChanges()
    {
        var entries = this.ChangeTracker.Entries()
            .Where(x => x.State is not EntityState.Unchanged and not EntityState.Detached)
            .Where(x => x.Entity is BaseEntity);

        foreach (var entry in entries)
        {
            var obj = (BaseEntity)entry.Entity;

            obj.RecordCreatedDate = DateTimeProvider.Now;

            if (entry.State == EntityState.Modified)
                obj.RecordModifiedDate = DateTimeProvider.Now;
        }
    }
}
