namespace PlantHub.Web.Components.PhToast;

public class PhToastService
{
    public event Action<PhToastMessage>? OnShow;

    public void Show(string text,
                     PhToastLevel level = PhToastLevel.Info,
                     TimeSpan? duration = null)
    {
        OnShow?.Invoke(new PhToastMessage
        {
            Text = text,
            Level = level,
            Duration = duration ?? TimeSpan.FromSeconds(3)
        });
    }
}