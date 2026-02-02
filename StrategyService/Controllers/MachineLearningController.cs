namespace BN.PROJECT.StrategyService
{
    [Route("[controller]")]
    [ApiController]
    //[AuthorizeUser(["user", "admin"])]
    public class MachineLearningController : ControllerBase
    {
        private readonly ILogger<MachineLearningController> _logger;
        private readonly IStrategyRepository _strategyRepository;
        private readonly IFinAIServiceClient _finAIServiceClient;

        // GlobalExceptionMiddleware for logging exceptions

        public MachineLearningController(ILogger<MachineLearningController> logger,
            IStrategyRepository strategyRepository,
            IFinAIServiceClient finAIServiceClient)
        {
            _logger = logger;
            _strategyRepository = strategyRepository;
            _finAIServiceClient = finAIServiceClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetMLModels()
        {
            var models = await _finAIServiceClient.GetLgbModels();
            if (models == null)
            {
                return NotFound();
            }
            return Ok(models);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMLModelById(string id)
        {
            var model = await _finAIServiceClient.GetLgbModelById(id);
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }

        //[HttpPost("run-test")]
        //public async Task<IActionResult> RunMLModelTest([FromBody] StrategySettingsModel settings)
        //{

        //}
    
    }
}
