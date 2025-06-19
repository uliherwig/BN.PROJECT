namespace BN.PROJECT.AlpacaService;


public class AlpacaHub : Hub, IAlpacaHub
{
    private readonly IDatabase _redisDatabase;

    public AlpacaHub(IConnectionMultiplexer redis)
    {
        _redisDatabase = redis.GetDatabase();
    }

    public async Task RegisterRateFeed(string userId)
    {
        var senderId = Context.ConnectionId;
        await _redisDatabase.StringSetAsync(userId, Context.ConnectionId);
    }

    public async Task<string> GetConnectinId(string userId)
    {
        var connectionId = await _redisDatabase.StringGetAsync(userId);
        return connectionId.ToString();
    }
    public async Task RetrieveMessageHistory() =>
        await Clients.Caller.SendAsync("MessageHistory", "");

    public async Task SendMessageToRecipient(string quoteMessage)
    {
        var qm = quoteMessage.FromJson<QuoteMessage>();
        var connectionId = await _redisDatabase.StringGetAsync(qm.UserId.ToString());
        if (!string.IsNullOrEmpty(connectionId.ToString()))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveQuote", quoteMessage);
        }
    }
}

