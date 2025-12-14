namespace PlantHub.Web.Lib;

public class NotificationSettings
{
    public bool AllowPush { get; set; } = false;
    public bool PushEnabled { get; set; } = false;
    public string[] PushNotifyServices { get; set; } = Array.Empty<string>();
}
