using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Parquet;
using Parquet.Data;
using Microsoft.Data.Analysis;
using Microsoft.Extensions.Logging;

namespace BN.PROJECT.StrategyService;

public class RedisParquetService : IRedisParquetService
{
    private readonly IDatabaseAsync _redis;
    private readonly ILogger<RedisParquetService> _logger;

    public RedisParquetService(
        IConnectionMultiplexer redis,
        ILogger<RedisParquetService> logger)
    {
        _redis = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<DataFrame> ReadParquetFromRedisAsync(string key)
    {
        _logger.LogInformation($"Reading Parquet data for key: {key}");

        // 1. Fetch raw bytes from Redis asynchronously
        byte[] parquetBytes = await _redis.StringGetAsync(key);

        if (parquetBytes == null || parquetBytes.Length == 0)
        {
            _logger.LogError($"No data found for key: {key}");
            throw new Exception($"No data found for key: {key}");
        }

        _logger.LogDebug($"Fetched {parquetBytes.Length} bytes from Redis.");

        // 2. Deserialize bytes into a Parquet file in memory
        using (var stream = new MemoryStream(parquetBytes))
        {
            // 3. Read the Parquet file into a DataFrame asynchronously
            using (var parquetReader = await ParquetReader.CreateAsync(stream))
            {
                var dataColumns = await parquetReader.ReadEntireRowGroupAsync();

                // Convert Parquet columns to DataFrame columns
                var columns = new List<DataFrameColumn>();
                foreach (var col in dataColumns)
                {
                    var field = col.Field;
                    Array data = col.Data;
                    DataFrameColumn dfCol;

                    if (field.ClrNullableIfHasNullsType == typeof(int))
                        dfCol = new PrimitiveDataFrameColumn<int>(field.Name, (IEnumerable<int>)data);
                    else if (field.ClrNullableIfHasNullsType == typeof(long))
                        dfCol = new PrimitiveDataFrameColumn<long>(field.Name, (IEnumerable<long>)data);
                    else if (field.ClrNullableIfHasNullsType == typeof(float))
                        dfCol = new PrimitiveDataFrameColumn<float>(field.Name, (IEnumerable<float>)data);
                    else if (field.ClrNullableIfHasNullsType == typeof(double))
                        dfCol = new PrimitiveDataFrameColumn<double>(field.Name, (IEnumerable<double>)data);
                    else if (field.ClrNullableIfHasNullsType == typeof(bool))
                        dfCol = new PrimitiveDataFrameColumn<bool>(field.Name, (IEnumerable<bool>)data);
                    else if (field.ClrNullableIfHasNullsType == typeof(string))
                        dfCol = new StringDataFrameColumn(field.Name, (IEnumerable<string>)data);
                    else if (field.ClrNullableIfHasNullsType == typeof(int?))
                        dfCol = new PrimitiveDataFrameColumn<int>(field.Name, (IEnumerable<int?>)data);
                    else if (field.ClrNullableIfHasNullsType == typeof(double?))
                        dfCol = new PrimitiveDataFrameColumn<double>(field.Name, (IEnumerable<double?>)data);
                    else
                        throw new Exception(field.ClrNullableIfHasNullsType.ToString());

                    columns.Add(dfCol);
                }
                return new DataFrame(columns);
            }
        }
    }
}
