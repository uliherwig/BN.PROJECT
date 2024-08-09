namespace BN.TRADER.AlpacaService
{
    [ApiController]
    [Route("[controller]")]
    public class AlpacaTradingController : ControllerBase
    {
        private readonly IAlpacaTradingService _alpacaTradingService;

        public AlpacaTradingController(IAlpacaTradingService alpacaTradingService)
        {
            _alpacaTradingService = alpacaTradingService;
        }

        [HttpGet("account")]
        public async Task<IActionResult> GetAccount()
        {
            var account = await _alpacaTradingService.GetAccountAsync();
            return Ok(account);
        }

        [HttpGet("assets")]
        public async Task<IActionResult> GetAssets()
        {
            var assets = await _alpacaTradingService.GetAssetsAsync();
            return Ok(assets);
        }

        [HttpGet("asset/{symbol}")]
        public async Task<IActionResult> GetAssetBySymbol(string symbol)
        {
            var asset = await _alpacaTradingService.GetAssetBySymbolAsync(symbol);
            return Ok(asset);
        }

        [HttpPost("order")]
        public async Task<IActionResult> CreateOrder([FromBody] string symbol, int quantity, bool isBuy, bool isMarket, string timeInForceString)
        {
            OrderQuantity qty = quantity;
            OrderSide side = isBuy ? OrderSide.Buy : OrderSide.Sell;
            OrderType orderType = isMarket ? OrderType.Market : OrderType.Limit;
            TimeInForce timeInForce = TimeInForce.Day;

            var order = await _alpacaTradingService.CreateOrderAsync(symbol, qty, side, orderType, timeInForce);
            return Ok(order);
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _alpacaTradingService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            var order = await _alpacaTradingService.GetOrderByIdAsync(orderId);
            return Ok(order);
        }

        [HttpDelete("order/{orderId}")]
        public async Task<IActionResult> CancelOrderById(Guid orderId)
        {
            var result = await _alpacaTradingService.CancelOrderByIdAsync(orderId);
            return Ok(result);
        }

        [HttpGet("positions")]
        public async Task<IActionResult> GetPositions()
        {
            var positions = await _alpacaTradingService.GetPositions();
            return Ok(positions);
        }

        [HttpGet("position/{symbol}")]
        public async Task<IActionResult> GetPositionsBySymbol(string symbol)
        {
            var position = await _alpacaTradingService.GetPositionsBySymbol(symbol);
            return Ok(position);
        }

        [HttpDelete("position/{symbol}")]
        public async Task<IActionResult> ClosePosition(string symbol)
        {
            var result = await _alpacaTradingService.ClosePosition(symbol);
            return Ok(result);
        }
    }
}