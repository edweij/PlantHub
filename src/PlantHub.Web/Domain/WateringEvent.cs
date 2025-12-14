namespace PlantHub.Web.Domain;

public class WateringEvent
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public Plant Plant { get; set; } = default!;
    public DateTime TimestampUtc { get; set; }
}
