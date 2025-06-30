
namespace BN.PROJECT.NotificationService;


public interface INotificationHub
{
    Task<string> GetConnectionId(string userId);
}