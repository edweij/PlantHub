using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlantHub.Web.Domain;

namespace PlantHub.Web.Infrastructure.Configurations;

public class WateringGroupConfiguration : IEntityTypeConfiguration<WateringGroup>
{
    public void Configure(EntityTypeBuilder<WateringGroup> b)
    {
        b.ToTable("WateringGroups");

        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        b.HasIndex(x => x.Name)
            .IsUnique();

        b.Property(x => x.Description)
            .HasMaxLength(500);

        // Seasonal base intervals (required, >= 1)
        b.Property(x => x.IntervalDaysSummer).IsRequired();
        b.Property(x => x.IntervalDaysWinter).IsRequired();

        // Summer window (inclusive months)
        b.Property(x => x.SummerStartMonth).HasDefaultValue(5);
        b.Property(x => x.SummerEndMonth).HasDefaultValue(9);

        // Guard rails (optional, >= 1 if set)
        b.Property(x => x.MinDaysBetween);
        b.Property(x => x.MaxDaysBetween);

        // Audit
        b.Property(x => x.CreatedUtc).IsRequired();
        b.Property(x => x.UpdatedUtc);

        // Check constraints for data integrity
        b.ToTable(t =>
        {
            t.HasCheckConstraint("CK_WG_Interval_Summer", "IntervalDaysSummer >= 1");
            t.HasCheckConstraint("CK_WG_Interval_Winter", "IntervalDaysWinter >= 1");
            t.HasCheckConstraint("CK_WG_Month_Start", "SummerStartMonth BETWEEN 1 AND 12");
            t.HasCheckConstraint("CK_WG_Month_End", "SummerEndMonth BETWEEN 1 AND 12");
            t.HasCheckConstraint("CK_WG_Min_Positive", "(MinDaysBetween IS NULL) OR (MinDaysBetween >= 1)");
            t.HasCheckConstraint("CK_WG_Max_Positive", "(MaxDaysBetween IS NULL) OR (MaxDaysBetween >= 1)");
            t.HasCheckConstraint("CK_WG_MinLEMax",
                "(MinDaysBetween IS NULL) OR (MaxDaysBetween IS NULL) OR (MinDaysBetween <= MaxDaysBetween)");
        });
    }
}
