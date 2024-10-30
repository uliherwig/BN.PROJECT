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
    public async Task<IActionResult> AddUserSettingsAsync([FromBody] UserSettings userSettings)
    {
        try
        {
            if (userSettings == null)
            {
                return BadRequest("UserSettings cannot be null");
            }

            await _alpacaRepository.AddUserSettingsAsync(userSettings);
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user settings");
        }
        return Ok(false);

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
    public async Task<IActionResult> UpdateUserSettingsAsync([FromBody] UserSettings userSettings)
    {
        try
        {
            if (userSettings == null)
            {
                return BadRequest("UserSettings cannot be null");
            }

            await _alpacaRepository.UpdateUserSettingsAsync(userSettings);
            return Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user settings");
        }
        return Ok(false);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteUserSettingsAsync([FromBody] UserSettings userSettings)
    {
        if (userSettings == null)
        {
            return BadRequest("UserSettings cannot be null");
        }

        await _alpacaRepository.DeleteUserSettingsAsync(userSettings);
        return NoContent();
    }
}
