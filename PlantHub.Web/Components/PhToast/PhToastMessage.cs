namespace PlantHub.Web.Components.PhToast;

public enum PhToastLevel
{
    Info,
    Success,
    Warning,
    Error
}

public class PhToastMessage
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Text { get; init; } = "";
    public PhToastLevel Level { get; init; } = PhToastLevel.Info;
    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(3);
}
