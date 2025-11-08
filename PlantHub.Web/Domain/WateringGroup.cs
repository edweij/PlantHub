namespace PlantHub.Web.Domain;

public record WateringGroup
{
    public int Id { get; init; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Säsongsbaserade grundintervall (obligatoriska, >=1)
    public int IntervalDaysSummer { get; set; } = 7;  // ex: 5–7 på tropiska
    public int IntervalDaysWinter { get; set; } = 14; // ex: 14–21 på sukulenter

    // Vilka månader räknas som sommar? (inklusive gränserna)
    // Default Sverige: maj–september
    public int SummerStartMonth { get; set; } = 5; // 1..12
    public int SummerEndMonth { get; set; } = 9; // 1..12

    // Räcken (valfria, >=1 om satta)
    public int? MinDaysBetween { get; set; }
    public int? MaxDaysBetween { get; set; }

    public ICollection<Plant> Plants { get; init; } = new List<Plant>();

    // Audit
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
}
