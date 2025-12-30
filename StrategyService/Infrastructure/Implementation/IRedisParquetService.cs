using Microsoft.Data.Analysis;

namespace BN.PROJECT.StrategyService
{
    public interface IRedisParquetService
    {
        Task<DataFrame> ReadParquetFromRedisAsync(string key);
    }
}