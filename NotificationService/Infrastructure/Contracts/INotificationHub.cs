
namespace BN.PROJECT.NotificationService;


public interface INotificationHub
{
    Task<string> GetConnectinId(string userId);
}