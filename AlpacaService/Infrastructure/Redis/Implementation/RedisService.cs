using System.Globalization;

namespace BN.PROJECT.AlpacaService;

public class RedisService : IRedisService
{
    //private readonly IConfiguration _configuration;
    //private readonly IAlpacaDataService _alpacaDataService;
    //private readonly IAlpacaRepository _alpacaRepository;
    //private readonly IHubContext<AlpacaHub> _hubContext;
    //private readonly IHubContext<AlpacaHub> _hubContext;
    private readonly ILogger<RedisService> _logger;
    private readonly IRedisStreamPublisher _redisStreamPublisher;
    private readonly IDatabase _redisDatabase;
    private readonly IConnectionMultiplexer _redis;
 
    public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger, IRedisStreamPublisher redisStreamPublisher)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _redisDatabase = _redis.GetDatabase();
        _logger = logger;
        _redisStreamPublisher = redisStreamPublisher;
    }
    public async Task<string> GetStringAsync(string key)
    {
        return await _redisDatabase.StringGetAsync(key);
    }
    public async Task SetStringAsync(string key, string value)
    {
        Expiration expiry = TimeSpan.FromHours(1);
        await _redisDatabase.StringSetAsync(key, value);
    }
    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _redisDatabase.KeyExistsAsync(key);
    }
    public async Task DeleteKeyAsync(string key)
    {
        await _redisDatabase.KeyDeleteAsync(key);
    }

    public async Task PublishTradesToStream(string symbol, List<AlpacaTrade> trades, int? maxLength)
    {
        var streamKey = RedisUtilities.GetTradesStreamKey(symbol);
        foreach (var trade in trades)
        {
            var fields = new[]
            {
                new NameValueEntry("symbol", trade.Symbol),
                new NameValueEntry("timestampUtc", trade.TimestampUtc.ToString("O", CultureInfo.InvariantCulture)),
                new NameValueEntry("price", trade.Price.ToString(CultureInfo.InvariantCulture)),
                new NameValueEntry("size", trade.Size.ToString(CultureInfo.InvariantCulture)),
                new NameValueEntry("exchange", trade.Exchange),
                new NameValueEntry("tape", trade.Tape),
                new NameValueEntry("tradeId", trade.TradeId.ToString(CultureInfo.InvariantCulture)),
            };

            try
            {
                await _redisStreamPublisher.AddAsync(streamKey, fields, maxLength);
            }
            catch (Exception e)
            {
                // Stream push is a best-effort side channel; DB persistence above must not be affected by Redis failures.
                _logger.LogError(e, "Failed to push trade {TradeId} for {Symbol} to Redis stream", trade.TradeId, symbol);
            }
        }
    }
}
