namespace BN.PROJECT.AlpacaService
{
    [ApiController]
    [Route("[controller]")]
    public class AlpacaTradingController : ControllerBase
    {
        private readonly IAlpacaTradingService _alpacaTradingService;
        private readonly IDbRepository _dbRepository;

        public AlpacaTradingController(
            IAlpacaTradingService alpacaTradingService,
            IDbRepository dbRepository)
        {
            _alpacaTradingService = alpacaTradingService;
            _dbRepository = dbRepository;
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
            var assets = await _dbRepository.GetAssets();
            return Ok(assets);
        }

        [HttpGet("asset/{symbol}")]
        public async Task<IActionResult> GetAssetBySymbol(string symbol)
        {
            var asset = await _alpacaTradingService.GetAssetBySymbolAsync(symbol);
            return Ok(asset);
        }

        [HttpPost("asset/{symbol}")]
        public async Task<IActionResult> ToggleAssetSelected(string symbol)
        {
            var result = await _dbRepository.ToggleAssetSelected(symbol);
            return Ok(result);
        }

        [HttpPost("market-order")]
        public async Task<IActionResult> CreateMarketOrder(string symbol, int quantity, bool isBuy)
        {
            OrderQuantity qty = quantity;
            OrderSide side = isBuy ? OrderSide.Buy : OrderSide.Sell;
            var order = await _alpacaTradingService.CreateOrderAsync(symbol, qty, side, OrderType.Market, TimeInForce.Day);
            if (order != null)
            {
                await _dbRepository.AddOrderAsync(order);
            }
            return Ok(order);
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders(OrderStatusFilter orderStatusFilter)
        {
            var orders = await _alpacaTradingService.GetAllOrdersAsync(orderStatusFilter);
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
            var positions = await _alpacaTradingService.GetAllOpenPositions();
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
            var result = await _alpacaTradingService.ClosePositionOrder(symbol);
            return Ok(result);
        }
    }
}