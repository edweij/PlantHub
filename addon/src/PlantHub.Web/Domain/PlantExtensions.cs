namespace PlantHub.Web.Domain;

public static class PlantExtensions
{
    public static DateTime? GetLastWatered(this Plant plant)
    {
        return plant.WateringEvents
            .OrderByDescending(e => e.TimestampUtc)
            .Select(e => (DateTime?)e.TimestampUtc)
            .FirstOrDefault();
    }

    public static bool NeedsWatering(this Plant plant)
    {
        var overdue = plant.GetDaysOverdue();
        return overdue.HasValue && overdue > 0;
    }

    public static DateTime? ComputeNextWaterDate(this Plant plant)
    {
        if (plant.WateringGroup is null)
            return null;

        var today = DateTime.Today;
        var last = plant.GetLastWatered() ?? DateTime.MinValue;

        var isSummer = DomainHelper.IsSummer(today, plant.WateringGroup);
        var days = isSummer
            ? plant.WateringGroup.IntervalDaysSummer
            : plant.WateringGroup.IntervalDaysWinter;

        if (days <= 0)
            return null;

        return last.AddDays(days);
    }

    public static int? GetDaysOverdue(this Plant plant)
    {
        var next = plant.ComputeNextWaterDate();
        if (next is null)
            return null;

        var today = DateTime.Today;
        var diff = (today - next.Value.Date).Days;

        return diff > 0 ? diff : 0;
    }
}
