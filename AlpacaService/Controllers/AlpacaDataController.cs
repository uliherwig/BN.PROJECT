namespace BN.PROJECT.AlpacaService;

[ApiController]
[Route("[controller]")]
public class AlpacaDataController : ControllerBase
{
    private readonly IAlpacaDataService _alpacaDataService;

    public AlpacaDataController(IAlpacaDataService alpacaDataService)
    {
        _alpacaDataService = alpacaDataService;
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
}