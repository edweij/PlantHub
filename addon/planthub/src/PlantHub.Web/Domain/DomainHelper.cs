namespace PlantHub.Web.Domain;

public class DomainHelper
{
    public static bool IsSummer(DateTime now, WateringGroup g)
    {
        var m = now.Month;

        // Om intervallet inte korsar årsskifte (t.ex. 5..9)
        if (g.SummerStartMonth <= g.SummerEndMonth)
            return m >= g.SummerStartMonth && m <= g.SummerEndMonth;

        // Korsar årsskifte (t.ex. 11..3)
        return m >= g.SummerStartMonth || m <= g.SummerEndMonth;
    }

    /// <summary>
    /// Cylinder pot (round straight walls):
    /// volume_ml = π * (d/2)^2 * h. Input in cm, returns ml.
    /// </summary>
    public static int CylinderVolumeMl(double diameterCm, double heightCm)
    {
        if (diameterCm <= 0 || heightCm <= 0) return 0;
        var r = diameterCm / 2.0;
        var cm3 = Math.PI * r * r * heightCm;
        return (int)Math.Round(cm3, MidpointRounding.AwayFromZero); // 1 cm³ == 1 ml
    }

    /// <summary>
    /// Square/rectangular straight pot:
    /// volume_ml = w * d * h. Input in cm, returns ml.
    /// </summary>
    public static int BoxVolumeMl(double widthCm, double depthCm, double heightCm)
    {
        if (widthCm <= 0 || depthCm <= 0 || heightCm <= 0) return 0;
        var cm3 = widthCm * depthCm * heightCm;
        return (int)Math.Round(cm3, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Approximate tapered round pot (frustum of a cone).
    /// topD, bottomD, height in cm. Returns ml.
    /// V = (π*h/12) * (D1^2 + D1*D2 + D2^2)
    /// </summary>
    public static int TaperedRoundVolumeMl(double topDiameterCm, double bottomDiameterCm, double heightCm)
    {
        if (topDiameterCm <= 0 || bottomDiameterCm <= 0 || heightCm <= 0) return 0;
        var cm3 = Math.PI * heightCm / 12.0 *
                  (topDiameterCm * topDiameterCm +
                   topDiameterCm * bottomDiameterCm +
                   bottomDiameterCm * bottomDiameterCm);
        return (int)Math.Round(cm3, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Clamp safe path for image storage (basic defense against traversal).
    /// Accepts null/empty; returns normalized or null.
    /// </summary>
    public static string? NormalizeImagePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        var trimmed = path.Trim();

        // Very light normalization: reject path traversal fragments.
        if (trimmed.Contains("..")) return null;

        // You can add stricter rules or map to a fixed base directory elsewhere.
        return trimmed.Replace('\\', '/');
    }
}
