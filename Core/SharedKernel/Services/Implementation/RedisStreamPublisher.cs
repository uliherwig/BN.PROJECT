namespace BN.PROJECT.Core;

public class RedisStreamPublisher : IRedisStreamPublisher
{
    private readonly IConnectionMultiplexer _redis;

    public RedisStreamPublisher(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<string> AddAsync(string streamKey, IEnumerable<NameValueEntry> fields, int? maxLength = null)
    {
        var database = _redis.GetDatabase();
        // Approximate trimming (~ MAXLEN) keeps the stream bounded without an expensive exact trim.
        var id = await database.StreamAddAsync(streamKey, fields.ToArray(), maxLength: maxLength, useApproximateMaxLength: true);
        return id.ToString();
    }
}
