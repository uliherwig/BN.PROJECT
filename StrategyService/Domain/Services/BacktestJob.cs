using BN.PROJECT.Core;

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


        // read dataframe from redis using parquet file
        var key = strategySettings.StrategyType == StrategyEnum.IndicatorBased ?
            $"strategy_test_data:{strategySettings.Id}" :
            $"ml_test_data:{strategySettings.Id}";
        var dataFrame = await _redisParquetService.ReadParquetFromRedisAsync(key);

        _logger.LogInformation($"DataFrame for strategy {strategySettings.Id} has {dataFrame.Rows.Count} rows and {dataFrame.Columns.Count} columns.");

        var data = StrategyOperations.DataFrameToBarSignalList(dataFrame);

        try
        {
            var positions = new List<PositionModel>();

            decimal equity = 10000m;
            var sl = strategySettings.StopLossPercent;
            var tp = strategySettings.TakeProfitPercent;
            var asset = strategySettings.Asset;
            var spreadPerTrade = strategySettings.SpreadPerTrade;
            var overnightFeeRate = strategySettings.OvernightFeeRate;
            var reverseTrade = strategySettings.ReverseTrade;
            var closePositionsEod = strategySettings.ClosePositionEod;



            Guid positionId = Guid.Empty;
            decimal totalProfit = 0m;





            // iterate over dataframe rows

            foreach (var row in data)
            {
                var currentTimestamp = row.Timestamp;
                var currentSignal = row.Signal;
                var currentPrice = row.Close;

                var openPosition = positions.FirstOrDefault(p => p.StampClosed == DateTime.MinValue);

                if (openPosition != null)
                {
                    var posSideMultiplier = openPosition.Side == SideEnum.Buy ? 1 : -1;
                    var currentReturn = (currentPrice - openPosition.PriceOpen) * posSideMultiplier / openPosition.PriceOpen;

                    // calculate overnight holds
                    var entryDate = openPosition.StampOpened.Date;
                    var currentDate = currentTimestamp.Date;
                    var overNightHolds = (currentDate - entryDate).Days;

                    var totalFee = 2 * spreadPerTrade * openPosition.Quantity;
                    var overnightFee = overNightHolds * overnightFeeRate * openPosition.PriceOpen * openPosition.Quantity;
                    var profit = (currentPrice - openPosition.PriceOpen) * posSideMultiplier * openPosition.Quantity - totalFee - overnightFee;

                    // check take profit and stop loss               
                    if (currentReturn >= tp || currentReturn <= -sl)
                    {

                        // close position
                        openPosition.ClosePosition(
                            currentTimestamp,
                            currentPrice,
                            currentReturn >= tp ? "TP" : "SL",
                            profit
                        );
                        totalProfit += profit;
                        equity += profit;

                        // reset ??

                    }

                    // check reverse trade
                    if (reverseTrade && ((openPosition.Side == SideEnum.Buy && currentSignal == -1) || (openPosition.Side == SideEnum.Sell && currentSignal == 1)))
                    {
                        // close position
                        openPosition.ClosePosition(
                            currentTimestamp,
                            currentPrice,
                            "REV",
                            profit
                        );
                        totalProfit += profit;
                        equity += profit;
                    }

                    // check end of day close
                    if (closePositionsEod)
                    {
                        if (currentTimestamp.Date > openPosition.StampOpened.Date)
                        {
                            // close position
                            openPosition.ClosePosition(
                                currentTimestamp,
                                currentPrice,
                                "EOD",
                                profit
                            );
                            totalProfit += profit;
                            equity += profit;
                        }
                    }

                }

                // check to open new position
                if (openPosition == null)
                {
                    if (currentSignal == 1 || currentSignal == -1)
                    {
                        // open new position
                        var side = currentSignal == 1 ? SideEnum.Buy : SideEnum.Sell;
                        var quantity = strategySettings.Quantity;
                        var takeProfitPrice = side == SideEnum.Buy ?
                            currentPrice * (1 + tp) :
                            currentPrice * (1 - tp);
                        var stopLossPrice = side == SideEnum.Buy ?
                            currentPrice * (1 - sl) :
                            currentPrice * (1 + sl);
                        var newPosition = PositionExtensions.CreatePosition(
                            strategySettings.Id,
                            asset,
                            quantity,
                            side,
                            currentPrice,
                            stopLossPrice,
                            takeProfitPrice,
                            currentTimestamp,
                            spreadPerTrade,
                            overnightFeeRate,
                            strategySettings.StrategyParams
                        );
                        positions.Add(newPosition);
                    }

                }

                // close last open position at the end of the backtest
                if (openPosition != null)
                {
                    var posSideMultiplier = openPosition.Side == SideEnum.Buy ? 1 : -1;
                    var currentReturn = (currentPrice - openPosition.PriceOpen) * posSideMultiplier / openPosition.PriceOpen;
                    // calculate overnight holds
                    var entryDate = openPosition.StampOpened.Date;
                    var currentDate = currentTimestamp.Date;
                    var overNightHolds = (currentDate - entryDate).Days;
                    var totalFee = 2 * spreadPerTrade * openPosition.Quantity;
                    var overnightFee = overNightHolds * overnightFeeRate * openPosition.PriceOpen * openPosition.Quantity;
                    var profit = (currentPrice - openPosition.PriceOpen) * posSideMultiplier * openPosition.Quantity - totalFee - overnightFee;
                    // close position
                    openPosition.ClosePosition(
                        currentTimestamp,
                        currentPrice,
                        "END",
                        profit
                    );
                    totalProfit += profit;
                    equity += profit;
                }
            }

            strategySettings.StampEnd = DateTime.UtcNow.ToUniversalTime();
            await _strategyRepository.UpdateStrategyAsync(strategySettings);
            await _strategyRepository.AddPositionsAsync(positions);

            // Notify user
            var notificationTopic = RedisUtilities.GetChannelName(RedisChannelEnum.Notification);
            var notificationMessage = NotificationMessageFactory.CreateNotificationMessage(
                strategySettings.UserId,
                NotificationEnum.BacktestStop
            );
            await _publisher.PublishAsync(notificationTopic, notificationMessage.ToJson());

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during backtest execution for strategy {strategySettings.Id}");



        }

    }
}