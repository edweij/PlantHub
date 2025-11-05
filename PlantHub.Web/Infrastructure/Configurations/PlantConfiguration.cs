using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlantHub.Web.Domain;

namespace PlantHub.Web.Infrastructure.Configurations;

public class PlantConfiguration : IEntityTypeConfiguration<Plant>
{
    public void Configure(EntityTypeBuilder<Plant> b)
    {
        b.ToTable("Plants");

        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        b.HasIndex(x => x.Name); // Not unique; users may reuse names

        b.Property(x => x.LatinName).HasMaxLength(200);
        b.Property(x => x.Area).HasMaxLength(120);

        // Pot volume in ml (positive if set)
        b.Property(x => x.PotVolumeMl);

        // Image path (relative or absolute)
        b.Property(x => x.ImagePath).HasMaxLength(400);

        // Watering mode enum
        b.Property(x => x.Mode).IsRequired();

        // Optional relation to WateringGroup when Mode == Schedule
        b.HasOne(x => x.WateringGroup)
            .WithMany()
            .HasForeignKey(x => x.WateringGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        // Sensor bindings / thresholds
        b.Property(x => x.SensorEntityId).HasMaxLength(200);
        b.Property(x => x.MoistureLowPercent);
        b.Property(x => x.MoistureHighPercent);

        // Tracking & audit
        b.Property(x => x.LastWateredUtc);
        b.Property(x => x.CreatedUtc).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd().IsRequired();
        b.Property(x => x.UpdatedUtc);

        // DB-level constraints
        b.ToTable(t =>
        {
            // If Schedule (0), require group
            t.HasCheckConstraint("CK_Plant_ScheduleGroup",
                "(Mode <> 0) OR (WateringGroupId IS NOT NULL)");

            // PotVolumeMl > 0 if set
            t.HasCheckConstraint("CK_Plant_PotVolume_Positive",
                "(PotVolumeMl IS NULL) OR (PotVolumeMl > 0)");

            // Moisture thresholds 0..100 if set
            t.HasCheckConstraint("CK_Plant_MoistureLow_0_100",
                "(MoistureLowPercent IS NULL) OR (MoistureLowPercent BETWEEN 0 AND 100)");
            t.HasCheckConstraint("CK_Plant_MoistureHigh_0_100",
                "(MoistureHighPercent IS NULL) OR (MoistureHighPercent BETWEEN 0 AND 100)");
        });
    }
}
