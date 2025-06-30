namespace BN.PROJECT.Core;

public class NotificationMessage
{
    public Guid UserId { get; set; }
    public NotificationEnum NotificationType { get; set; }
    public string JsonData { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}