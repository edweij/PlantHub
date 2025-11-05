using Microsoft.EntityFrameworkCore;
using PlantHub.Web.Domain;
using PlantHub.Web.Infrastructure.Configurations;

namespace PlantHub.Web.Infrastructure;

public class PlantHubDbContext : DbContext
{
    public PlantHubDbContext(DbContextOptions<PlantHubDbContext> options) : base(options) { }

    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<WateringGroup> WateringGroups => Set<WateringGroup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PlantConfiguration());
        modelBuilder.ApplyConfiguration(new WateringGroupConfiguration());
    }

    public override int SaveChanges()
    {
        TouchUpdatedUtc();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TouchUpdatedUtc();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void TouchUpdatedUtc()
    {
        var now = DateTime.UtcNow;

        foreach (var e in ChangeTracker.Entries<Plant>())
        {
            if (e.State == EntityState.Added)
                e.Entity.UpdatedUtc = e.Entity.CreatedUtc; // or now
            else if (e.State == EntityState.Modified)
                e.Entity.UpdatedUtc = now;
        }

        foreach (var e in ChangeTracker.Entries<WateringGroup>())
        {
            if (e.State == EntityState.Added)
                e.Entity.UpdatedUtc = e.Entity.CreatedUtc; // or now
            else if (e.State == EntityState.Modified)
                e.Entity.UpdatedUtc = now;
        }
    }
}
