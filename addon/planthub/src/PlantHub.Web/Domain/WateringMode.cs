namespace PlantHub.Web.Domain;

public enum WateringMode
{
    // Schedule-based reminders (e.g., every N days)
    Schedule = 0,
    // Sensor-based (e.g., use soil moisture thresholds)
    Sensor = 1,
    // Manual-only (no reminders)
    Manual = 2
}
