namespace BN.PROJECT.IdentityService;

[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;
    private readonly IIdentityRepository _identityRepository;
    private readonly IKeycloakServiceClient _keycloakServiceClient;

    public AccountController(
        IConfiguration configuration, ILogger<AccountController> logger,
        IIdentityRepository identityRepository,
        IKeycloakServiceClient keycloakServiceClient)
    {
        _configuration = configuration;
        _logger = logger;
        _identityRepository = identityRepository;
        _keycloakServiceClient = keycloakServiceClient;
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest signInRequest)
    {
        var response = await _keycloakServiceClient.SignIn(signInRequest);

        if ((bool)response.Success)
        {
            var user = await _identityRepository.GetUserByNameAsync(signInRequest.Username);
            if (user != null)
            {
                var session = new Session
                {
                    SessionId = Guid.NewGuid(),
                    UserId = user.UserId,
                    CreatedAt = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    SignedOutAt = DateTime.MinValue
                };
                await _identityRepository.AddSessionAsync(session);
            }
        }
        return Ok(response);
    }

    [HttpPost("sign-out")]
    public async Task<IActionResult> Logout([FromBody] SignOutRequest signOutRequest)
    {
        var response = await _keycloakServiceClient.SignOut(signOutRequest);
        if ((bool)response.Success)
        {
            var claims = JwtTokenDecoder.DecodeJwtToken(signOutRequest.RefreshToken);
            var userId = new Guid(claims["sub"]);
            var session = await _identityRepository.GetSessionByUserIdAsync(userId);
            if (session != null)
            {
                session.SignedOutAt = DateTime.UtcNow;
                session.LastActive = DateTime.UtcNow;
                await _identityRepository.UpdateSessionAsync(session);
            }
        }
        return Ok(response);
    }


    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
    {

        var response = await _keycloakServiceClient.RefreshToken(refreshTokenRequest);
        if (!string.IsNullOrEmpty(response.RefreshToken))
        {
            var claims = JwtTokenDecoder.DecodeJwtToken(response.RefreshToken);
            var userId = new Guid(claims["sub"]);
            var session = await _identityRepository.GetSessionByUserIdAsync(userId);
            if (session != null)
            {
                session.ExpiresAt = DateTime.UtcNow.AddMinutes(5);
                session.LastActive = DateTime.UtcNow;
                await _identityRepository.UpdateSessionAsync(session);
            }
        }
        return Ok(response);
    }

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest signUpRequest)
    {
        var response = await _keycloakServiceClient.SignUp(signUpRequest);

        if (response != null)
        {
            if (response.Success == true)
            {
                var user = new User
                {
                    UserId = new Guid(response.UserId),
                    Username = signUpRequest.Username,
                    Email = signUpRequest.Email,
                    FirstName = signUpRequest.FirstName,
                    LastName = signUpRequest.LastName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsEmailVerified = false
                };
                await _identityRepository.AddUserAsync(user);
            }

            return Ok(response);
        }
        return BadRequest();

    }

    [KeycloakAuthorize("admin")]
    [HttpGet("test-auth")]
    public IActionResult TestAuthorization()
    {
        // Only accessible to users with the "admin" role
        return Ok(new { Message = "Access granted!" });
    }
}