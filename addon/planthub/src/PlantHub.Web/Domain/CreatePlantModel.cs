using System.ComponentModel.DataAnnotations;

namespace PlantHub.Web.Domain;

public class CreatePlantModel : IValidatableObject
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? LatinName { get; set; }

    public string? AreaId { get; set; }

    [Required]
    public WateringMode Mode { get; set; } = WateringMode.Schedule;

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
        if (Mode == WateringMode.Schedule && !WateringGroupId.HasValue)
        {
            yield return new ValidationResult(
                "Select a watering group for schedule-based plants.",
                new[] { nameof(WateringGroupId) });
        }

        if (Mode == WateringMode.Sensor)
        {
            if (string.IsNullOrWhiteSpace(SensorEntityId))
            {
                yield return new ValidationResult(
                    "Select a soil moisture sensor.",
                    new[] { nameof(SensorEntityId) });
            }

            if (!MoistureLowPercent.HasValue)
            {
                yield return new ValidationResult(
                    "Enter the moisture percentage that should trigger a notification.",
                    new[] { nameof(MoistureLowPercent) });
            }
        }

        if (MoistureLowPercent.HasValue && (MoistureLowPercent < 0 || MoistureLowPercent > 100))
        {
            yield return new ValidationResult(
                "Low moisture threshold must be between 0 and 100.",
                new[] { nameof(MoistureLowPercent) });
        }

        if (MoistureHighPercent.HasValue && (MoistureHighPercent < 0 || MoistureHighPercent > 100))
        {
            yield return new ValidationResult(
                "High moisture threshold must be between 0 and 100.",
                new[] { nameof(MoistureHighPercent) });
        }

        if (MoistureLowPercent.HasValue && MoistureHighPercent.HasValue && MoistureHighPercent <= MoistureLowPercent)
        {
            yield return new ValidationResult(
                "Recovery threshold must be higher than the low moisture threshold.",
                new[] { nameof(MoistureLowPercent), nameof(MoistureHighPercent) });
        }

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

