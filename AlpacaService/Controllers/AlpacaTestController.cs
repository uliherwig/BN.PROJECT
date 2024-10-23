namespace BN.PROJECT.AlpacaService
{
    [Route("[controller]")]
    [ApiController]
    public class AlpacaTestController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IAlpacaRepository _alpacaRepository;

        public AlpacaTestController(
            IWebHostEnvironment env, IAlpacaRepository alpacaRepository)
        {
            _env = env;
            _alpacaRepository = alpacaRepository;
        }

        [HttpGet("historical-bars/{symbol}")]
        public async Task<IActionResult> GetHistoricalBarsBySymbol(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var bars = await _alpacaRepository.GetHistoricalBars(symbol, startDate.ToUniversalTime(), endDate.ToUniversalTime());
                return Ok(bars);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

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