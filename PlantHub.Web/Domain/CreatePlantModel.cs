using System.ComponentModel.DataAnnotations;

namespace PlantHub.Web.Domain;

public class CreatePlantModel
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? LatinName { get; set; }

    public string? AreaId { get; set; }

    [Required]
    public WateringMode Mode { get; set; } = WateringMode.Schedule;

    [Required]
    public int? WateringGroupId { get; set; }

    [Range(1, 100_000)]
    public int? PotVolumeMl { get; set; }

    [Range(1, 200)]
    public double? PotDiameterCm { get; set; }

    [Range(1, 200)]
    public double? PotHeightCm { get; set; }    

    [MaxLength(500)]
    public string? Notes { get; set; }

    public string? SensorEntityId { get; set; }
    public double? MoistureLowPercent { get; set; }
    public double? MoistureHighPercent { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Tillåt allt tomt (ingen pottinfo), ELLER en komplett specifikation.
        var hasVol = PotVolumeMl.HasValue;
        var hasDims = PotDiameterCm.HasValue || PotHeightCm.HasValue;

        if (hasDims && !(PotDiameterCm.HasValue && PotHeightCm.HasValue))
        {
            yield return new ValidationResult(
                "Provide both diameter and height, or leave both empty.",
                new[] { nameof(PotDiameterCm), nameof(PotHeightCm) });
        }

        if (hasVol && hasDims)
        {
            yield return new ValidationResult(
                "Fill either volume or (diameter + height), not both.",
                new[] { nameof(PotVolumeMl), nameof(PotDiameterCm), nameof(PotHeightCm) });
        }
    }
}

public static class PlantMapping
{
    public static Plant ToEntity(this CreatePlantModel m)
    {
        int? potVolume = m.PotVolumeMl;

        if (!potVolume.HasValue &&
            m.PotDiameterCm is > 0 and var d &&
            m.PotHeightCm is > 0 and var h)
        {
            var r = d / 2.0;
            var volume = Math.PI * r * r * h;
            potVolume = (int)Math.Round(volume);
        }

        return new Plant
        {
            Name = m.Name.Trim(),
            LatinName = string.IsNullOrWhiteSpace(m.LatinName) ? null : m.LatinName.Trim(),
            AreaId = string.IsNullOrWhiteSpace(m.AreaId) ? null : m.AreaId.Trim(),
            Mode = m.Mode,
            WateringGroupId = m.Mode == WateringMode.Schedule ? m.WateringGroupId : null,
            PotVolumeMl = potVolume,
            Notes = string.IsNullOrWhiteSpace(m.Notes) ? null : m.Notes.Trim(),
            SensorEntityId = m.Mode == WateringMode.Sensor ? m.SensorEntityId : null,
            MoistureLowPercent = m.Mode == WateringMode.Sensor ? m.MoistureLowPercent : null,
            MoistureHighPercent = m.Mode == WateringMode.Sensor ? m.MoistureHighPercent : null,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow
        };
    }
}

