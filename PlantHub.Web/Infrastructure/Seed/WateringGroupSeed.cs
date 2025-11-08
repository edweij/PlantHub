using Microsoft.EntityFrameworkCore;
using PlantHub.Web.Domain;

namespace PlantHub.Infrastructure.Seed;

public static class WateringGroupSeed
{
    public static void SeedWateringGroups(this ModelBuilder modelBuilder)
    {
        var created = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<WateringGroup>().HasData(
            // 1: Succulents & Cacti
            new WateringGroup
            {
                Id = 1,
                Name = "Succulents & Cacti",
                Description = "Very drought-tolerant plants. Let soil dry completely. Typical: aloe, cacti, succulents.",
                IntervalDaysSummer = 14,   // ca var 14–21 dag
                IntervalDaysWinter = 21,   // ca var 21–30 dag
                SummerStartMonth = 5,
                SummerEndMonth = 9,
                MinDaysBetween = 14,
                MaxDaysBetween = 30,
                CreatedUtc = created
            },

            // 2: Drought-Tolerant Green Foliage
            new WateringGroup
            {
                Id = 2,
                Name = "Drought-Tolerant Green Foliage",
                Description = "Green foliage that prefers to dry slightly between waterings. Typical: monstera, pothos, umbrella plant.",
                IntervalDaysSummer = 7,    // ca var 7–10 dag
                IntervalDaysWinter = 10,   // ca var 10–14 dag
                SummerStartMonth = 5,
                SummerEndMonth = 9,
                MinDaysBetween = 7,
                MaxDaysBetween = 14,
                CreatedUtc = created
            },

            // 3: Thirsty Foliage & Flowering
            new WateringGroup
            {
                Id = 3,
                Name = "Thirsty Foliage & Flowering",
                Description = "Plants that like lightly and evenly moist soil. Typical: coleus (palettblad), indoor pelargonium.",
                IntervalDaysSummer = 4,    // ca var 3–5 dag
                IntervalDaysWinter = 6,    // ca var 5–7 dag
                SummerStartMonth = 5,
                SummerEndMonth = 9,
                MinDaysBetween = 3,
                MaxDaysBetween = 7,
                CreatedUtc = created
            },

            // 4: Mediterranean Trees
            new WateringGroup
            {
                Id = 4,
                Name = "Mediterranean Trees",
                Description = "Citrus and olive in larger pots. Let dry lightly between waterings, adjust more between seasons.",
                IntervalDaysSummer = 5,    // ca var 3–7 dag
                IntervalDaysWinter = 14,   // ca var 10–21 dag
                SummerStartMonth = 5,
                SummerEndMonth = 9,
                MinDaysBetween = 3,
                MaxDaysBetween = 21,
                CreatedUtc = created
            }
        );
    }
}
