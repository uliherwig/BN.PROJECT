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

    //[KeycloakAuthorize("user")]
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
        var bar = await _alpacaDataService.GetLatestQuoteBySymbol(symbol);
        return Ok(bar);
    }

    [HttpGet("trades/{symbol}")]
    public async Task<IActionResult> GetTradesBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var trades = await _alpacaDataService.GetTradesBySymbol(symbol, startDate, endDate);
        return Ok(trades);
    }

    [HttpPost("save-historicalBars/{symbol}")]
    public async Task<IActionResult> PostHistoricalBarsBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var bars = await _alpacaDataService.GetHistoricalBarsBySymbol(symbol, startDate, endDate, BarTimeFrame.Minute);
        return Ok(bars);
    }
}