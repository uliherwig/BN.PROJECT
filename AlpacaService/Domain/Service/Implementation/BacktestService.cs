namespace BN.PROJECT.AlpacaService;

public class BacktestService
{
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly ILogger<BacktestService> _logger;
    private readonly PositionManager _positionManager;

    public BacktestService(IAlpacaRepository alpacaRepository, ILogger<BacktestService> logger, PositionManager positionManager)
    {
        _alpacaRepository = alpacaRepository;
        _positionManager = positionManager;
        _logger = logger;
    }

    public async Task RunBacktest(BacktestSettings testSettings)
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
            var bars = await _alpacaRepository.GetHistoricalBars(symbol, stamp, stamp.Add(timeFrame));
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
