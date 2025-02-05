namespace BN.PROJECT.AlpacaService;

[Route("[controller]")]
[ApiController]
public class UserSettingsController : ControllerBase
{
    private readonly IAlpacaRepository _alpacaRepository;
    private readonly ILogger<UserSettingsController> _logger;

    public UserSettingsController(IAlpacaRepository alpacaRepository, ILogger<UserSettingsController> logger)
    {
        _alpacaRepository = alpacaRepository;
        _logger = logger;
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

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserSettingsAsync(string userId)
    {
        var userSettings = await _alpacaRepository.GetUserSettingsAsync(userId);
        if (userSettings == null)
        {
            return NotFound(ErrorCode.NotFound);
        }

        return Ok(userSettings);
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
    public async Task<IActionResult> DeleteUserSettingsAsync([FromBody] UserSettingsModel userSettings)
    {
        if (userSettings == null)
        {
            return BadRequest("UserSettings cannot be null");
        }

        await _alpacaRepository.DeleteUserSettingsAsync(userSettings);
        return NoContent();
    }
}