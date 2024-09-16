namespace BN.PROJECT.DataService;

[ApiController]
[Route("[controller]")]
public class AlpacaDbController : ControllerBase
{
    private readonly ILogger<AlpacaDbController> _logger;
    private readonly IAlpacaRepository _dbRepository;

    public AlpacaDbController(ILogger<AlpacaDbController> logger, IAlpacaRepository dbRepository)
    {
        _logger = logger;
        _dbRepository = dbRepository;
    }

    // GET: api/alpaca/assets
    [HttpGet("assets")]
    public async Task<ActionResult<List<AlpacaAsset>>> GetAssets()
    {
        var assets = await _dbRepository.GetAssets();
        return Ok(assets);
    }

    // POST: api/alpaca/assets
    [HttpPost("assets")]
    public async Task<IActionResult> AddAssets([FromBody] List<AlpacaAsset> assets)
    {
        await _dbRepository.AddAssetsAsync(assets);
        return Ok();
    }

    // GET: api/alpaca/assets/{symbol}
    [HttpGet("assets/{symbol}")]
    public async Task<ActionResult<AlpacaAsset>> GetAsset(string symbol)
    {
        var asset = await _dbRepository.GetAsset(symbol);
        if (asset == null)
        {
            return NotFound();
        }
        return Ok(asset);
    }

    // GET: api/alpaca/bars/latest/{symbol}
    [HttpGet("bars/latest/{symbol}")]
    public async Task<ActionResult<AlpacaBar>> GetLatestBar(string symbol)
    {
        var bar = await _dbRepository.GetLatestBar(symbol);
        if (bar == null)
        {
            return NotFound();
        }
        return Ok(bar);
    }

    // GET: api/alpaca/bars/historical/{symbol}
    [HttpGet("bars/historical/{symbol}")]
    public async Task<ActionResult<List<AlpacaBar>>> GetHistoricalBars(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var bars = await _dbRepository.GetHistoricalBars(symbol, startDate, endDate);
        return Ok(bars);
    }

    // POST: api/alpaca/bars
    [HttpPost("bars")]
    public async Task<IActionResult> AddBars([FromBody] List<AlpacaBar> bars)
    {
        await _dbRepository.AddBarsAsync(bars);
        return Ok();
    }

    // POST: api/alpaca/orders
    [HttpPost("orders")]
    public async Task<IActionResult> AddOrder([FromBody] AlpacaOrder order)
    {
        await _dbRepository.AddOrderAsync(order);
        return Ok();
    }

    // GET: api/alpaca/orders/{id}
    [HttpGet("orders/{id}")]
    public async Task<ActionResult<AlpacaOrder>> GetOrder(int id)
    {
        var order = await _dbRepository.GetOrderAsync(id);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    // PUT: api/alpaca/orders/{id}
    [HttpPut("orders/{id}")]
    public async Task<IActionResult> UpdateOrder(string id, [FromBody] AlpacaOrder order)
    {
        if (id != order.OrderId.ToString())
        {
            return BadRequest();
        }

        await _dbRepository.UpdateOrderAsync(order);
        return Ok();
    }
}
