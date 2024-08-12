namespace BN.TRADER.AlpacaService
{
    [ApiController]
    [Route("[controller]")]
    public class AlpacaDataController : ControllerBase
    {
        private readonly IAlpacaDataService _alpacaDataService;

        public AlpacaDataController(IAlpacaDataService alpacaDataService)
        {
            _alpacaDataService = alpacaDataService;
        }

        [HttpGet("historicalBars/{symbol}")]
        public async Task<IActionResult> GetHistoricalBarsBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var bars = await _alpacaDataService.GetHistoricalBarsBySymbol(symbol, startDate, endDate, BarTimeFrame.Minute);
            return Ok(bars);
        }

        [HttpGet("latestBar/{symbol}")]
        public async Task<IActionResult> GetLatestBarBySymbol(string symbol)
        {
            var bar = await _alpacaDataService.GetLatestBarBySymbol(symbol);
            return Ok(bar);
        }

        [HttpGet("trades/{symbol}")]
        public async Task<IActionResult> GetTradesBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var trades = await _alpacaDataService.GetTradesBySymbol(symbol, startDate, endDate);
            return Ok(trades);
        }

        [HttpGet("quotes/{symbol}")]
        public async Task<IActionResult> GetQuotesBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var quotes = await _alpacaDataService.GetQuotesBySymbol(symbol, startDate, endDate);
            return Ok(quotes);
        }

        [HttpPost("save-historicalBars/{symbol}")]
        public async Task<IActionResult> PostHistoricalBarsBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var bars = await _alpacaDataService.GetHistoricalBarsBySymbol(symbol, startDate, endDate, BarTimeFrame.Minute);
            return Ok(bars);
        }
    }
}