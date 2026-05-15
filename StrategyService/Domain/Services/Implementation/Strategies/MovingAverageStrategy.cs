
namespace BN.PROJECT.StrategyService;

public class MovingAverageStrategy : IIndicatorService
{
    public struct IndicatorPoint
    {
        public DateTime Date { get; set; }
        public double? Value { get; set; }
    }

    public async Task<List<BarSignalModel>> StartTest(StrategySettingsModel strategySettings, List<Quote> quotes)
    {

        var indicator = strategySettings.IndicatorType;

        var movingAvgParams = JsonConvert.DeserializeObject<MovAvgModel>(strategySettings.StrategyParams);
        
        if (movingAvgParams == null)
            throw new ArgumentNullException(nameof(movingAvgParams), "Strategy parameters cannot be null.");

        switch (indicator)
        {
            case IndicatorEnum.SMA:
            case IndicatorEnum.EMA:
            case IndicatorEnum.WMA:
                return  EvaluateCrossovers(indicator,movingAvgParams, quotes);
            default:
                throw new NotSupportedException($"Indicator {indicator} is not supported.");
        }
    }


    private List<BarSignalModel> EvaluateCrossovers(IndicatorEnum indicator, MovAvgModel movingAvgParams, List<Quote> quotes)
    {
        var shortSma = new List<IndicatorPoint>();
        var longSma = new List<IndicatorPoint>();
        switch (indicator)
        {
            case IndicatorEnum.SMA:
                 shortSma = quotes.GetSma(movingAvgParams.ShortPeriod).Select(ma => new IndicatorPoint { Date = ma.Date, Value = ma.Sma }).ToList();
                 longSma = quotes.GetSma(movingAvgParams.LongPeriod).Select(ma => new IndicatorPoint { Date = ma.Date, Value = ma.Sma }).ToList();
                 break;
            case IndicatorEnum.EMA:
                shortSma = quotes.GetEma(movingAvgParams.ShortPeriod).Select(ma => new IndicatorPoint { Date = ma.Date, Value = ma.Ema }).ToList();
                longSma = quotes.GetEma(movingAvgParams.LongPeriod).Select(ma => new IndicatorPoint { Date = ma.Date, Value = ma.Ema }).ToList();
                break;
            case IndicatorEnum.WMA:
                shortSma = quotes.GetWma(movingAvgParams.ShortPeriod).Select(ma => new IndicatorPoint { Date = ma.Date, Value = ma.Wma }).ToList();
                longSma = quotes.GetWma(movingAvgParams.LongPeriod).Select(ma => new IndicatorPoint { Date = ma.Date, Value = ma.Wma }).ToList();
                break;
            default:
                throw new NotSupportedException($"Indicator {indicator} is not supported.");
        }
        List<BarSignalModel> data = new List<BarSignalModel>();
        var count = quotes.Count;
        for (int i = 1; i < count; i++)
        {
            var signal = 0;
            double prevShort = shortSma[i - 1].Value ?? 0;
            double prevLong = longSma[i - 1].Value ?? 0;

            double currShort = shortSma[i].Value ?? 0;
            double currLong = longSma[i].Value ?? 0;

            if (prevShort <= prevLong && currShort > currLong)
            {
                signal = 1;
            }
            else if (prevShort >= prevLong && currShort < currLong)
            {
                signal = -1;
            }
            data.Add(new BarSignalModel(i, quotes[i].Date, quotes[i].Open, quotes[i].High, quotes[i].Low, quotes[i].Close, signal));
        }

        return data;
    }

    public List<PositionModel> GetPositions()
    {
        throw new NotImplementedException();
    }

    public Task<TestResult> GetTestResult()
    {
        throw new NotImplementedException();
    }

    public bool CanHandle(IndicatorEnum indicator) =>
     indicator == IndicatorEnum.SMA
     || indicator == IndicatorEnum.EMA
     || indicator == IndicatorEnum.WMA;
}