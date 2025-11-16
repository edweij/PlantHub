// Domain/Plant.cs
namespace PlantHub.Web.Domain;


public record Plant
{
    public int Id { get; init; }

    // Required, display name
    public string Name { get; set; } = null!;

    // Optional Latin name for lookups (Wikipedia, etc.)
    public string? LatinName { get; set; }

    // Store HA Area Id (eller tom tills vidare)
    public string? AreaId { get; set; }

    // Pot volume in milliliters (1 cm³ == 1 ml). Nullable until known.
    public int? PotVolumeMl { get; set; }

    // Relative or absolute path to an image (e.g., /data/images/plants/123.jpg)
    public string? ImagePath { get; set; }

    // Free-form notes
    public string? Notes { get; set; }

    // Active flag instead of hård delete
    public bool IsActive { get; set; } = true;


    // How this plant is watered
    public WateringMode Mode { get; set; } = WateringMode.Schedule;

    // ---- Schedule mode ----
    public int? WateringGroupId { get; set; }
    public WateringGroup? WateringGroup { get; set; }

    // ---- Sensor mode ----
    public string? SensorEntityId { get; set; }
    public double? MoistureLowPercent { get; set; }
    public double? MoistureHighPercent { get; set; }

    // ---- Tracking ----
    public DateTime? LastWateredUtc { get; set; }

    // Audit
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
}
