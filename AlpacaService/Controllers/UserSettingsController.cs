namespace BN.PROJECT.AlpacaService;

[Route("[controller]")]
[ApiController]
[AuthorizeUser(["user","admin"])]
public class UserSettingsController : ControllerBase
{
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly ILogger<UserSettingsController> _logger;

    public UserSettingsController(IAlpacaRepository alpacaRepository, ILogger<UserSettingsController> logger)
    {
        _alpacaRepository = alpacaRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserSettingsAsync()
    {
        var userId = HttpContext.Items["UserId"]?.ToString();

        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId!);
        if (userSettings == null)
        {
            return base.NotFound(Core.BnErrorCode.NotFound);
        }

        return Ok(userSettings);
    }

    [HttpPost]
    public async Task<IActionResult> AddUserSettingsAsync([FromBody] UserSettingsModel userSettings)
    {
        if (userSettings == null)
        {
            return BadRequest("UserSettings cannot be null");
        }

        await _alpacaRepository.AddUserSettingsAsync(userSettings);
        return Ok(true);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUserSettingsAsync([FromBody] UserSettingsModel userSettings)
    {
        if (userSettings == null)
        {
            return BadRequest("UserSettings cannot be null");
        }

        await _alpacaRepository.UpdateUserSettingsAsync(userSettings);
        return Ok(true);
    }

    [HttpDelete]  
    public async Task<IActionResult> DeleteUserSettingsAsync()
    {
        var userId = HttpContext.Items["UserId"]?.ToString();

        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId!);
        if(userSettings != null)
        {
            await _alpacaRepository.DeleteUserSettingsAsync(userSettings);
        }       
        return Ok();
    }
}