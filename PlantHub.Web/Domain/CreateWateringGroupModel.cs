using System.ComponentModel.DataAnnotations;

namespace PlantHub.Web.Domain
{
    public class CreateWateringGroupModel
    {
        [Required]
        [StringLength(64)]
        public string Name { get; set; } = string.Empty;

        [StringLength(512)]
        public string? Description { get; set; }

        // Base intervals (required, >= 1)
        [Range(1, 60)]
        public int IntervalDaysSummer { get; set; } = 7;

        [Range(1, 60)]
        public int IntervalDaysWinter { get; set; } = 14;

        // Summer period (1..12)
        [Range(1, 12)]
        public int SummerStartMonth { get; set; } = 5;

        [Range(1, 12)]
        public int SummerEndMonth { get; set; } = 9;

        // Optional bounds
        [Range(1, 60)]
        public int? MinDaysBetween { get; set; }

        [Range(1, 60)]
        public int? MaxDaysBetween { get; set; }
    }

    public static class WateringGroupMapping
    {
        public static WateringGroup ToEntity(this CreateWateringGroupModel m)
        {
            // Optional: simple guard if min/max are "inverted"
            int? min = m.MinDaysBetween;
            int? max = m.MaxDaysBetween;
            if (min.HasValue && max.HasValue && min > max)
            {
                // swap so it's never totally nonsense
                (min, max) = (max, min);
            }

            return new WateringGroup
            {
                Name = m.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(m.Description) ? null : m.Description.Trim(),

                IntervalDaysSummer = m.IntervalDaysSummer,
                IntervalDaysWinter = m.IntervalDaysWinter,

                SummerStartMonth = m.SummerStartMonth,
                SummerEndMonth = m.SummerEndMonth,

                MinDaysBetween = min,
                MaxDaysBetween = max,

                // Plants = new List<Plant>() // handled by default
                // CreatedUtc sätts av default-init i entiteten
            };
        }
    }
}
