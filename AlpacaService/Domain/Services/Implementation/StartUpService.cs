namespace BN.PROJECT.AlpacaService;

public class StartUpService : IStartUpService
{
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IAlpacaDataService _alpacaDataService;


    public StartUpService(IAlpacaRepository alpacaRepository, IAlpacaDataService alpacaDataService)
    {
        _alpacaRepository = alpacaRepository;
        _alpacaDataService = alpacaDataService;
    }

    public async Task InitializeTradesStorage(List<string> assetsSelection)
    {

        var now = DateTime.UtcNow;
        var interval = TimeSpan.FromSeconds(20);
       
        var calendar = await _alpacaRepository.GetCalendarAsync(DateOnly.FromDateTime(now));
        if (calendar == null || calendar.Count == 0)
        {
            return;
        }
        // TradingOpen/TradingClose are stored in Eastern Time; convert to UTC to compare against stamp (UTC).
        var easternZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var tradingDate = calendar.First().TradingDate;
        var tradingOpen = TimeZoneInfo.ConvertTimeToUtc(
            tradingDate.ToDateTime(TimeOnly.FromTimeSpan(calendar.First().TradingOpen), DateTimeKind.Unspecified),
            easternZone).TimeOfDay;
        var tradingClose = TimeZoneInfo.ConvertTimeToUtc(
            tradingDate.ToDateTime(TimeOnly.FromTimeSpan(calendar.First().TradingClose), DateTimeKind.Unspecified),
            easternZone).TimeOfDay;
        foreach (var symbol in assetsSelection)
        {
            var latestTradeFromDb = await _alpacaRepository.GetLatestTrade(symbol);
            if (latestTradeFromDb != null)
            {
                continue;
            }
           
            var stamp = now.AddDays(-2);
            while (stamp < now)
            {
                if (stamp.TimeOfDay < tradingOpen || stamp.Add(interval).TimeOfDay >= tradingClose)
                {
                    stamp = stamp.Add(interval);
                    continue;
                }

                var intervalStamp = stamp.Add(interval);
                var trades = await _alpacaDataService.GetTradesBySymbol(symbol, stamp, intervalStamp);

                if (trades.Count > 0)
                {
                    await _alpacaRepository.AddTradesAsync(trades);
                }
                stamp = intervalStamp;
            }
        }
    }
}