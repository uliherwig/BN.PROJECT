namespace BN.PROJECT.NotificationService;


public class NotificationHub : Hub, INotificationHub
{
    private readonly IDatabase _redisDatabase;

    public NotificationHub(IConnectionMultiplexer redis)
    {
        _redisDatabase = redis.GetDatabase();
    }

    public async Task RegisterNotificationFeed(string userId)
    {       
        await _redisDatabase.StringSetAsync(userId, Context.ConnectionId);
    }

    public async Task<string> GetConnectionId(string userId)
    {
        var connectionId = await _redisDatabase.StringGetAsync(userId);
        return connectionId.ToString();
    }
    public async Task RetrieveMessageHistory() =>
        await Clients.Caller.SendAsync("MessageHistory", "");

    public async Task SendMessageToRecipient(string notificationMessage)
    {
        var qm = notificationMessage.FromJson<NotificationMessage>();
        var connectionId = await _redisDatabase.StringGetAsync(qm.UserId.ToString());
        if (!string.IsNullOrEmpty(connectionId.ToString()))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveNotification", notificationMessage);
        }
    }
}

