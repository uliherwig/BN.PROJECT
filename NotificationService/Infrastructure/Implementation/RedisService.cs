﻿namespace BN.PROJECT.NotificationService;


public class RedisService
{
    //private readonly ILogger<RedisService> _logger;
    //private readonly IConfiguration _configuration;
    //private readonly IAlpacaDataService _alpacaDataService;
    //private readonly IAlpacaRepository _alpacaRepository;
    //private readonly IKafkaProducerService _kafkaProducer;
    //private readonly IHubContext<AlpacaHub> _hubContext;
    private readonly IDatabase _redisDatabase;
    private readonly IConnectionMultiplexer _redis;
 
    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _redisDatabase = _redis.GetDatabase();
        
    }
    public async Task<string> GetStringAsync(string key)
    {
        return await _redisDatabase.StringGetAsync(key);
    }
    public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        await _redisDatabase.StringSetAsync(key, value, expiry);
    }
    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _redisDatabase.KeyExistsAsync(key);
    }
    public async Task DeleteKeyAsync(string key)
    {
        await _redisDatabase.KeyDeleteAsync(key);
    }
}
