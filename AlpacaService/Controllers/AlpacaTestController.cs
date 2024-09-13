namespace BN.PROJECT.AlpacaService
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlpacaTestController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IDbRepository _alpacaRepository;

        public AlpacaTestController(
            IWebHostEnvironment env,
            IDbRepository alpacaRepository)
        {
            _env = env;
            _alpacaRepository = alpacaRepository;
        }

        [HttpGet("historicalBars/{symbol}")]
        public async Task<IActionResult> GetHistoricalBarsBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            string path = Path.Combine(_env.ContentRootPath, "Assets", "alpaca-bars.json");

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var jsonString = await reader.ReadToEndAsync();
                    return Ok(jsonString);
                }
            }
        }

        [HttpGet("assets")]
        public async Task<IActionResult> GetAssets()
        {
            var assets = await _alpacaRepository.GetAssets();
            return Ok(assets);
        }

        [HttpGet("save-assets")]
        public async Task<IActionResult> SaveAssets()
        {
            string path = Path.Combine(_env.ContentRootPath, "Assets", "alpaca-assets.json");

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var jsonString = await reader.ReadToEndAsync();

                    var assets = JsonConvert.DeserializeObject<List<AlpacaAsset>>(jsonString);
                    if (assets != null)
                    {
                        await _alpacaRepository.AddAssetsAsync(assets);
                    }
                    return Ok();
                }
            }
        }
    }
}