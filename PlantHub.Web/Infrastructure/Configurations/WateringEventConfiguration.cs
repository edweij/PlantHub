using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlantHub.Web.Domain;

namespace PlantHub.Web.Infrastructure.Configurations;

public class WateringEventConfiguration : IEntityTypeConfiguration<WateringEvent>
{
    public void Configure(EntityTypeBuilder<WateringEvent> b)
    {
        b.ToTable("WateringEvents");

        b.HasKey(x => x.Id);

        b.Property(x => x.TimestampUtc)
            .IsRequired();

        b.HasOne(x => x.Plant)
            .WithMany(p => p.WateringEvents)
            .HasForeignKey(x => x.PlantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}