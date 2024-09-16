namespace BN.PROJECT.AlpacaService
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlpacaTestController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly DataServiceClient _dataServiceClient;

        public AlpacaTestController(
            IWebHostEnvironment env, DataServiceClient dataServiceClient)
        {
            _env = env;
            _dataServiceClient = dataServiceClient;
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
                        await _dataServiceClient.AddAssetsAsync(assets);
                    }
                    return Ok();
                }
            }
        }
    }
}