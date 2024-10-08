namespace BN.PROJECT.AlpacaService;


[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class AlpacaHistoryJob : IJob
{
    private readonly ILogger<AlpacaHistoryJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAlpacaDataService _alpacaDataService;
    private readonly IAlpacaRepository _alpacaRepository;

    public AlpacaHistoryJob(
        ILogger<AlpacaHistoryJob> logger,
        IConfiguration configuration,
        IAlpacaDataService alpacaDataService,
        IAlpacaRepository alpacaRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _alpacaDataService = alpacaDataService;
        _alpacaRepository = alpacaRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        JobKey key = context.JobDetail.Key;

        var assetsAsString = _configuration.GetValue<string>("Alpaca:TRADED_ASSETS") ?? string.Empty;
        if (string.IsNullOrEmpty(assetsAsString))
            throw new Exception("No assets defined in configuration");

        var assetsSelection = assetsAsString.Split(",").ToList();
        _logger.LogInformation("Instance " + key + " History Job start");

        await UpdateHistoricalBars(assetsSelection);

        // TODO filter data by timespan to avoid overloading
        //await UpdateHistoricalQuotes(assetsSelection);

        _logger.LogInformation("Instance " + key + " History Job end");
    }


    private async Task UpdateHistoricalBars(List<string> assetsSelection)
    {
        foreach (var symbol in assetsSelection)
        {
            _logger.LogInformation("Asset: " + symbol);

            var latestBarFromDb = await _alpacaRepository.GetLatestBar(symbol);
            var latestBarFromAlpaca = await _alpacaDataService.GetLatestBarBySymbol(symbol);

            var startDate = latestBarFromDb == null ? new DateTime(2024, 1, 1) : latestBarFromDb.T;

            while (startDate < latestBarFromAlpaca.T)
            {
                var endDate = startDate.AddDays(1);
                var bars = await _alpacaDataService.GetHistoricalBarsBySymbol(symbol, startDate, endDate, BarTimeFrame.Minute);
                _logger.LogInformation("Start: " + startDate.ToString() + "  Bars: " + bars.Count);

                if (bars.Count > 0)
                {
                    await _alpacaRepository.AddBarsAsync(bars);
                }

                startDate = startDate.AddDays(1);
            }
        }
    }

    private async Task UpdateHistoricalQuotes(List<string> assetsSelection)
    {
        foreach (var symbol in assetsSelection)
        {
            _logger.LogInformation("Asset: " + symbol);

            var latestQuoteFromDb = await _alpacaRepository.GetLatestQuote(symbol);
            var latestBarFromAlpaca = await _alpacaDataService.GetLatestQuoteBySymbol(symbol);

            var startDate = latestQuoteFromDb == null ? new DateTime(2024, 1, 1) : latestQuoteFromDb.TimestampUtc;

            while (startDate < latestBarFromAlpaca.TimestampUtc)
            {
                var endDate = startDate.AddDays(1);
                var quotes = await _alpacaDataService.GetQuotesBySymbol(symbol, startDate, endDate);
                _logger.LogInformation("Start: " + startDate.ToString() + "  Quotes: " + quotes.Count);

                if (quotes.Count > 0)
                {
                    await _alpacaRepository.AddQuotesAsync(quotes);
                    startDate = quotes.Last().TimestampUtc;
                } else
                {
                    startDate = startDate.AddDays(1);
                }               
            }
        }
    }
}