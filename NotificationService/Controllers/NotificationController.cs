namespace BN.PROJECT.NotificationService;

[Route("[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly ILogger<NotificationController> _logger;
    public NotificationController(ILogger<NotificationController> logger)
    {
        _logger = logger;
    }
    [HttpGet("test")]
    public IActionResult Test()
    {
        _logger.LogInformation("Test endpoint hit");
        return Ok("Notification Service is running");
    }
}
