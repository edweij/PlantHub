namespace PlantHub.Web.Lib;

public record HaEntityStateLite(
    string EntityId,
    string FriendlyName,
    string State,
    string? UnitOfMeasurement,
    string? DeviceClass);
