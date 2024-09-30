namespace BN.PROJECT.StrategyService;

public class StrategyService : IStrategyService
{

    private readonly ILogger<StrategyService> _logger;
    private readonly PositionManager _positionManager;
    private readonly IKafkaConsumerService _kafkaConsumerService;
    private BacktestSettings _testSettings;
    private TestProcessModel _tpm;


    public StrategyService(ILogger<StrategyService> logger,
        PositionManager positionManager,
        IKafkaConsumerService kafkaConsumerService)
    {
        _positionManager = positionManager;
        _logger = logger;
        _kafkaConsumerService = kafkaConsumerService;
    }

    public async Task RunStrategyTest(BacktestSettings testSettings)
    {
        _testSettings = testSettings;
        var topic = $"backtest-{testSettings.UserEmail.ToLower().Replace('@', '-').Replace('.', '-')}";

        //var kafkaDeleteMessagesService = new KafkaDeleteMessagesService();
        //await kafkaDeleteMessagesService.DeleteMessagesAsync(topic);

        _kafkaConsumerService.Start(topic);
        _kafkaConsumerService.MessageReceived += AddMessage;
    }




    public async Task InitializeStrategyTest()
    {
        _logger.LogInformation("Backtest started");
        var h = new Quote { Symbol = _testSettings.Symbol, AskPrice = 0.0m, BidPrice = 0.0m, TimestampUtc = _testSettings.StartDate };
        var l = new Quote { Symbol = _testSettings.Symbol, AskPrice = 10000.0m, BidPrice = 10000.0m, TimestampUtc = _testSettings.StartDate };
        _tpm = new TestProcessModel
        {
            Symbol = _testSettings.Symbol,
            High = h,
            Low = l,
            StartDate = _testSettings.StartDate.ToUniversalTime(),
            EndDate = DateTime.UtcNow,
            TimeFrame = TimeSpan.FromDays(1)
        };
        return;



        //while (stamp < endDate)
        //{
        //    var bars = new List<Bar>();
        //    if (bars.Count == 0)
        //    {
        //        stamp = stamp.Add(timeFrame).ToUniversalTime();
        //        continue;
        //    }

        //    var open = bars.First();
        //    var close = bars.Last();
        //    var high = bars.Max(b => b.H);
        //    var low = bars.Min(b => b.L);

        //    _logger.LogInformation($"#BT STAMP:{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");

        //    var innerStamp = open.T.ToUniversalTime();


        //    //while (innerStamp < close.T)
        //    //{

        //    //    var innerBars = bars.Where(b => b.T > innerStamp && b.T < innerStamp.Add(innerTimeFrame)).ToList();

        //    //    if (innerBars.Count == 0)
        //    //    {
        //    //        innerStamp = innerStamp.Add(innerTimeFrame).ToUniversalTime();
        //    //        continue;
        //    //    }

        //    //    var innerOpen = innerBars.First();
        //    //    var innerClose = innerBars.Last();
        //    //    var innerHigh = innerBars.Max(b => b.H);
        //    //    var innerLow = innerBars.Min(b => b.L);

        //    //    _logger.LogInformation($"#BT ########### STAMP:{innerOpen.T.ToShortDateString()}:{innerOpen.T.ToShortTimeString()} HIGH: {innerHigh:F2} LOW: {innerLow:F2}  // Open: {innerOpen.O:F2}, Close: {innerClose.C:F2}");

        //    //    innerStamp = innerStamp.Add(innerTimeFrame).ToUniversalTime();
        //    //}

        //    // check breakout high or low

        //    foreach (var bar in bars)
        //    {
        //        if (bar.C > prevHigh)
        //        {
        //            var p = _positionManager.CreatePosition(symbol, 1, Side.Buy, bar.C, low, bar.C + 2);

        //            _logger.LogInformation($"#BT BUY :{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");

        //        }
        //        if (bar.C < prevLow)
        //        {
        //            var p = _positionManager.CreatePosition(symbol, 1, Side.Sell, bar.C, high, bar.C - 2);
        //            _logger.LogInformation($"#BT SELL:{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");
        //        }

        //        var openPositionsLong = _positionManager.GetOpenPositionsBySide(Side.Buy);

        //        foreach (var position in openPositionsLong)
        //        {
        //            if (bar.C > position.TakeProfit)
        //            {
        //                _positionManager.ClosePosition(position.Id, bar.C, "Take Profit");
        //                _logger.LogInformation($"#BT CLOSE LONG TP:{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");
        //            }
        //            else if (bar.C < position.StopLoss)
        //            {
        //                _positionManager.ClosePosition(position.Id, bar.C, "Stop Loss");
        //                _logger.LogInformation($"#BT CLOSE LONG SL:{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");
        //            }
        //        }

        //        var openPositionsShort = _positionManager.GetOpenPositionsBySide(Side.Sell);
        //        foreach (var position in openPositionsShort)
        //        {
        //            if (bar.C < position.TakeProfit)
        //            {
        //                _positionManager.ClosePosition(position.Id, bar.C, "Take Profit");
        //                _logger.LogInformation($"#BT CLOSE SHORT TP:{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");
        //            }
        //            else if (bar.C > position.StopLoss)
        //            {
        //                _positionManager.ClosePosition(position.Id, bar.C, "Stop Loss");
        //                _logger.LogInformation($"#BT CLOSE SHORT SL:{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");
        //            }
        //        }
        //    }

        //    prevHigh = high;
        //    prevLow = low;

        //    stamp = stamp.Add(timeFrame).ToUniversalTime();
        //};
        //var pos = _positionManager.GetAllPositions();

        //var profit = pos.Sum(p => p.ProfitLoss);
        //_logger.LogInformation($"#BT POSITIONS: {_positionManager.GetAllPositions().Count}  Profit: {profit}");



    }
    public async Task EvaluateQuote(string message)
    {

        var quote = JsonConvert.DeserializeObject<Quote>(message);
        if (quote == null)
        {
            return;
        }
        var currentStamp = quote.TimestampUtc;
        if (currentStamp - _testSettings.StartDate < TimeSpan.FromDays(3))
        {
            _tpm.High = quote.AskPrice > _tpm.High.AskPrice ? quote : _tpm.High;
            _tpm.Low = quote.BidPrice < _tpm.Low.BidPrice ? quote : _tpm.Low;
            if (_tpm.High.AskPrice - _tpm.Low.AskPrice < 2m)
            {
                _logger.LogInformation($"#BT NO BREAKOUT: {_tpm.High.TimestampUtc.ToShortDateString()}:{_tpm.High.TimestampUtc.ToShortTimeString()} HIGH: {_tpm.High.AskPrice:F2} LOW: {_tpm.Low.BidPrice:F2}");

            }
            return;
        }
        // todo
        // check increase or decrease
        _tpm.IsIncreasing = _tpm.High.TimestampUtc > _tpm.Low.TimestampUtc;

        var dayHigh = _tpm.High.TimestampUtc.Day;
        var dayLow = _tpm.Low.TimestampUtc.Day;
        var day = currentStamp.Day;


        // if breakout - adjust prevHigh and prevLow
        // check breakout high if increase
        if (quote.AskPrice > _tpm.High.AskPrice)
        {

            var p = _positionManager.CreatePosition(_tpm.Symbol, 1, Side.Buy, quote.AskPrice, _tpm.Low.BidPrice, quote.AskPrice + 2);
            //if(p != null)
            //{
            //    _tpm.Positions.Add(p);
            //}

        }

        // check breakout low if decrease
        if (quote.BidPrice < _tpm.High.BidPrice)
        {
            var p = _positionManager.CreatePosition(_tpm.Symbol, 1, Side.Sell, quote.BidPrice, _tpm.High.AskPrice, quote.BidPrice - 2);
            //if (p != null)
            //{
            //    _tpm.Positions.Add(p);
            //}

        }

        var openPositionsLong = _positionManager.GetOpenPositionsBySide(Side.Buy);

        foreach (var position in openPositionsLong)
        {
            if (_tpm.High.AskPrice > position.TakeProfit)
            {
                _positionManager.ClosePosition(position.Id, _tpm.High.BidPrice, "Take Profit");
            }
            else if (_tpm.High.BidPrice < position.StopLoss)
            {
                _positionManager.ClosePosition(position.Id, _tpm.High.BidPrice, "Stop Loss");
            }
        }

        var openPositionsShort = _positionManager.GetOpenPositionsBySide(Side.Sell);
        foreach (var position in openPositionsShort)
        {
            if (_tpm.Low.BidPrice < position.TakeProfit)
            {
                _positionManager.ClosePosition(position.Id, _tpm.Low.AskPrice, "Take Profit");
            }
            else if (_tpm.Low.AskPrice > position.StopLoss)
            {
                _positionManager.ClosePosition(position.Id, _tpm.Low.AskPrice, "Stop Loss");
            }
        }

    }
    public async Task StopStrategyTest()
    {
        _logger.LogInformation("Backtest stopped");
        _kafkaConsumerService.Stop();
    }
    private async void AddMessage(string message)
    {
        if (message == "start")
        {
            _logger.LogInformation("Backtest started");
            await InitializeStrategyTest();
        }
        else if (message == "stop")
        {
            await StopStrategyTest();
        }
        else
        {
            await EvaluateQuote(message);
        }
    }


    public async Task BacktestWithBars(BacktestSettings testSettings)
    {
        var symbol = testSettings.Symbol;
        var startDate = new DateTime(2024, 1, 1);
        var endDate = DateTime.UtcNow;
        var timeFrame = TimeSpan.FromDays(1);
        var innerTimeFrame = TimeSpan.FromHours(1);


        var positions = new List<Position>();

        var stamp = startDate.ToUniversalTime();
        var end = stamp.Add(timeFrame).ToUniversalTime();
        //var barsFirstTimeFrame = await _alpacaRepository.GetHistoricalBars(symbol, stamp, end);
        //var prevHigh = barsFirstTimeFrame.Max(b => b.H);
        //var prevLow = barsFirstTimeFrame.Min(b => b.T);
        //stamp = stamp.Add(timeFrame);

        var prevHigh = 10000.0m;
        var prevLow = 0.0m;



        while (stamp < endDate)
        {
            var bars = new List<Bar>();
            if (bars.Count == 0)
            {
                stamp = stamp.Add(timeFrame).ToUniversalTime();
                continue;
            }

            var open = bars.First();
            var close = bars.Last();
            var high = bars.Max(b => b.H);
            var low = bars.Min(b => b.L);

            _logger.LogInformation($"#BT STAMP:{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");

            var innerStamp = open.T.ToUniversalTime();


            //while (innerStamp < close.T)
            //{

            //    var innerBars = bars.Where(b => b.T > innerStamp && b.T < innerStamp.Add(innerTimeFrame)).ToList();

            //    if (innerBars.Count == 0)
            //    {
            //        innerStamp = innerStamp.Add(innerTimeFrame).ToUniversalTime();
            //        continue;
            //    }

            //    var innerOpen = innerBars.First();
            //    var innerClose = innerBars.Last();
            //    var innerHigh = innerBars.Max(b => b.H);
            //    var innerLow = innerBars.Min(b => b.L);

            //    _logger.LogInformation($"#BT ########### STAMP:{innerOpen.T.ToShortDateString()}:{innerOpen.T.ToShortTimeString()} HIGH: {innerHigh:F2} LOW: {innerLow:F2}  // Open: {innerOpen.O:F2}, Close: {innerClose.C:F2}");

            //    innerStamp = innerStamp.Add(innerTimeFrame).ToUniversalTime();
            //}

            // check breakout high or low

            foreach (var bar in bars)
            {
                if (bar.C > prevHigh)
                {
                    var p = _positionManager.CreatePosition(symbol, 1, Side.Buy, bar.C, low, bar.C + 2);

                    _logger.LogInformation($"#BT BUY :{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");

                }
                if (bar.C < prevLow)
                {
                    var p = _positionManager.CreatePosition(symbol, 1, Side.Sell, bar.C, high, bar.C - 2);
                    _logger.LogInformation($"#BT SELL:{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");
                }

                var openPositionsLong = _positionManager.GetOpenPositionsBySide(Side.Buy);

                foreach (var position in openPositionsLong)
                {
                    if (bar.C > position.TakeProfit)
                    {
                        _positionManager.ClosePosition(position.Id, bar.C, "Take Profit");
                        _logger.LogInformation($"#BT CLOSE LONG TP:{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");
                    }
                    else if (bar.C < position.StopLoss)
                    {
                        _positionManager.ClosePosition(position.Id, bar.C, "Stop Loss");
                        _logger.LogInformation($"#BT CLOSE LONG SL:{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");
                    }
                }

                var openPositionsShort = _positionManager.GetOpenPositionsBySide(Side.Sell);
                foreach (var position in openPositionsShort)
                {
                    if (bar.C < position.TakeProfit)
                    {
                        _positionManager.ClosePosition(position.Id, bar.C, "Take Profit");
                        _logger.LogInformation($"#BT CLOSE SHORT TP:{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");
                    }
                    else if (bar.C > position.StopLoss)
                    {
                        _positionManager.ClosePosition(position.Id, bar.C, "Stop Loss");
                        _logger.LogInformation($"#BT CLOSE SHORT SL:{open.T.ToShortDateString()}:{open.T.ToShortTimeString()} HIGH: {high:F2} LOW: {low:F2}  // Open: {open.O:F2}, Close: {close.C:F2}");
                    }
                }
            }

            prevHigh = high;
            prevLow = low;

            stamp = stamp.Add(timeFrame).ToUniversalTime();
        };
        var pos = _positionManager.GetAllPositions();

        var profit = pos.Sum(p => p.ProfitLoss);
        _logger.LogInformation($"#BT POSITIONS: {_positionManager.GetAllPositions().Count}  Profit: {profit}");



    }

}
