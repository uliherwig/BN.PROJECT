namespace BN.PROJECT.AlpacaService;

public interface IRedisService
{
    Task<string> GetStringAsync(string key);
    Task SetStringAsync(string key, string value);
    Task<bool> KeyExistsAsync(string key);
    Task DeleteKeyAsync(string key);
    Task PublishTradesToStream(string symbol, List<AlpacaTrade> trades, int? maxLength);
}
