namespace BN.PROJECT.AlpacaService;

[ApiController]
[Route("[controller]")]
public class AlpacaDataController : ControllerBase
{
    private readonly IAlpacaDataService _alpacaDataService;
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly IStrategyTestService _strategyTestService;

    public AlpacaDataController(IAlpacaDataService alpacaDataService, IAlpacaRepository alpacaRepository, IStrategyTestService strategyTestService)
    {
        _alpacaDataService = alpacaDataService;
        _alpacaRepository = alpacaRepository;
        _strategyTestService = strategyTestService;
    }

    [HttpGet("clock")]
    public async Task<IActionResult> GetClockAsync()
    {
        var result = await _alpacaDataService.GetClockAsync();
        return Ok(result);
    }

    [HttpGet("interval-calendar")]
    public async Task<IActionResult> ListIntervalCalendarAsync(DateOnly startDate, DateOnly endDate = default)
    {
        var result = await _alpacaDataService.ListIntervalCalendarAsync(startDate, endDate == default ? DateOnly.FromDateTime(DateTime.UtcNow) : endDate);
        return Ok(result);
    }

    [HttpGet("assets")]
    public async Task<IActionResult> GetAssets()
    {
        var assets = await _alpacaRepository.GetAssets();
        return Ok(assets);
    }

    [HttpGet("asset/{symbol}")]
    public async Task<IActionResult> GetAssetBySymbol(string symbol)
    {
        var asset = await _alpacaDataService.GetAssetBySymbolAsync(symbol);
        return Ok(asset);
    }

    [HttpGet("historical-bars/{symbol}")]
    public async Task<IActionResult> GetHistoricalBarsBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var bars = await _alpacaDataService.GetHistoricalBarsBySymbol(symbol, startDate, endDate, BarTimeFrame.Minute);
        return Ok(bars);
    }

    [HttpGet("latest-bar/{symbol}")]
    public async Task<IActionResult> GetLatestBarBySymbol(string symbol)
    {
        var bar = await _alpacaDataService.GetLatestBarBySymbol(symbol);
        return Ok(bar);
    }

    [HttpGet("historical-quotes/{symbol}")]
    public async Task<IActionResult> GetHistoricalQuotesBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var quotes = await _alpacaDataService.GetQuotesBySymbol(symbol, startDate, endDate);
        return Ok(quotes);
    }

    [HttpGet("latest-quote/{symbol}")]
    public async Task<IActionResult> GetLatestQuoteBySymbol(string symbol)
    {
        var quote = await _alpacaDataService.GetLatestQuoteBySymbol(symbol);
        return Ok(quote);
    }


    [HttpGet("historical-trades/{symbol}")]
    public async Task<IActionResult> GetHistoricalTradesBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var trades = await _alpacaDataService.GetTradesBySymbol(symbol, startDate, endDate);
        return Ok(trades);
    }

    [HttpGet("latest-trades/{symbol}")]
    public async Task<IActionResult> GetLatestTradeBySymbol(string symbol)
    {
        var trade = await _alpacaDataService.GetLatestTradeBySymbol(symbol);
        return Ok(trade);
    }


    [HttpPost("store-to-redis")]
    public async Task<IActionResult> StoreToRedis([FromBody] string asset)
    {
        await _strategyTestService.StoreBarsToRedis(asset);
        return Ok();
    }

    [HttpGet("save-assets")]
    public async Task<IActionResult> SaveAssets(string newAssets)
    { 
        var assets = JsonConvert.DeserializeObject<List<AlpacaAsset>>(newAssets);
        if (assets != null)
        {
            await _alpacaRepository.AddAssetsAsync(assets);
        }
        return Ok();
    }

}