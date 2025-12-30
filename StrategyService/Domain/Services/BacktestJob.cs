using StackExchange.Redis;

namespace BN.PROJECT.StrategyService;

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class BacktestJob : IJob
{
    private readonly ILogger<BacktestJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRedisPublisher _publisher;
    private readonly IStrategyServiceStore _serviceStore;
    private readonly IDatabase _redisDatabase;
    private readonly IRedisParquetService _redisParquetService;

    public BacktestJob(
        ILogger<BacktestJob> logger,
        IConfiguration configuration,
        IStrategyRepository strategyRepository,
        IServiceProvider serviceProvider,
        IStrategyServiceStore strategyServiceStore,
        IConnectionMultiplexer redis,
        IRedisPublisher publisher,
        IRedisParquetService redisParquetService
        )
    {
        _logger = logger;
        _configuration = configuration;
        _strategyRepository = strategyRepository;
        _serviceProvider = serviceProvider;
        _serviceStore = strategyServiceStore;
        _redisDatabase = redis.GetDatabase();
        _publisher = publisher;
        _redisParquetService = redisParquetService;

    }

    public async Task Execute(IJobExecutionContext context)
    {
        // get strategy settings
        // get quotes
        // create a strategy message
        // create the strategy service
        // start the backtest
        // evaluate quotes
        // get positions
        // get test result
        // to db: update strategy settings with end time
        // to db: save positions

        var dataMap = context.JobDetail.JobDataMap;
        var strategyId = (Guid)dataMap.WrappedMap["strategyId"];

        var strategySettings = await _strategyRepository.GetStrategyByIdAsync(strategyId);
        if (strategySettings == null)
        {
            _logger.LogError($"Strategy with ID {strategyId} not found.");
            return;
        }


        var notificationTopic = RedisUtilities.GetChannelName(RedisChannelEnum.Notification);
        var notificationMessage = NotificationMessageFactory.CreateNotificationMessage(
            strategySettings.UserId,
            NotificationEnum.BacktestStart
        );
        await _publisher.PublishAsync(notificationTopic, notificationMessage.ToJson());


        // read dataframe from redis using parquet file

        var key = $"strategy_test:{strategySettings.Id}";
        var dataFrame = await _redisParquetService.ReadParquetFromRedisAsync(key);

        _logger.LogInformation($"DataFrame for strategy {strategySettings.Id} has {dataFrame.Rows.Count} rows and {dataFrame.Columns.Count} columns.");

        try
        {


            decimal equity = 10000m;
            var sl = strategySettings.StopLossPercent;
            var tp = strategySettings.TakeProfitPercent;
            var asset = strategySettings.Asset;
            var spreadPerTrade = strategySettings.SpreadPerTrade;
            var overnightFeeRate = strategySettings.OvernightFeeRate;
            var reverseTrade = strategySettings.ReverseTrade;
            var closePositionsEod = strategySettings.ClosePositionEod;


            bool positionOpen = false;
            decimal entryPrice = 0m;
            int positionType = 0;
            Guid positionId = Guid.Empty;
            decimal profit = 0m;
            int overnightHolds = 0;
            int? entryIdx = null;
            DateTime? entryDate = null;

            var signalColId = $"{strategySettings.StrategyType.ToString().ToLower()}_signal";















        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during backtest execution for strategy {strategySettings.Id}");



        }

    }
}