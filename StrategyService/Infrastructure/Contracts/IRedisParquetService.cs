namespace BN.PROJECT.StrategyService;

public interface IRedisParquetService
{
    Task<DataFrame> ReadParquetFromRedisAsync(string key);
}