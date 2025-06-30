namespace BN.PROJECT.NotificationService;

[Route("[controller]")]
[ApiController]
[AuthorizeUser(["user", "admin"])]

public class NotificationController : ControllerBase
{
    private readonly ILogger<NotificationController> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IDatabase _redisDatabase;
    public NotificationController(
        ILogger<NotificationController> logger,
        IHubContext<NotificationHub> hubContext,
        IConnectionMultiplexer redis)
    {
        _logger = logger;
        _hubContext = hubContext;
        _redisDatabase = redis.GetDatabase();
    }
    [HttpGet("test")]
    public async Task<IActionResult> TestAsync([FromQuery] string notificationType )
    {
        _logger.LogInformation("Test signalR function");

        var userId = HttpContext.Items["UserId"]?.ToString();

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("UserId is not provided in the request context.");
        }
        if (!Enum.TryParse<NotificationEnum>(notificationType, true, out var notificationEnum))
        {
            return BadRequest("Ungültiger NotificationType.");
        }

        var notificationMessage = new NotificationMessage
        {
            UserId = new Guid(userId),
            NotificationType = notificationEnum
        };

        var connectionId = await _redisDatabase.StringGetAsync(userId.ToString());
        if (!string.IsNullOrEmpty(connectionId.ToString()))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notificationMessage.ToJson());

        }

        return Ok("Notification Service is running");
    }
}
