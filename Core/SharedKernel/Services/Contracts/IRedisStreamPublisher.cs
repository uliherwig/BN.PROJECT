namespace BN.PROJECT.Core;

public interface IRedisStreamPublisher
{
    Task<string> AddAsync(string streamKey, IEnumerable<NameValueEntry> fields, int? maxLength = null);
}
