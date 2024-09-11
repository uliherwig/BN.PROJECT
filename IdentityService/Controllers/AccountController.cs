namespace BN.TRADER.IdentityService;

[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IKeycloakService _keycloakConfiguration;

    public AccountController(
        IConfiguration configuration, ILogger<AccountController> logger, IHttpClientFactory httpClientFactory, IKeycloakService keycloakConfiguration)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _keycloakConfiguration = keycloakConfiguration;
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest signInRequest)
    {
        var response = await _keycloakConfiguration.SignIn(signInRequest);
        return Ok(response);
    }

    [HttpPost("sign-out")]
    public async Task<IActionResult> Logout([FromBody] SignOutRequest signOutRequest)
    {
        var response = await _keycloakConfiguration.SignOut(signOutRequest);
        return Ok(response);
    }


    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var authority = _configuration["Keycloak:Authority"] ?? string.Empty;
        var clientId = _configuration["Keycloak:ClientId"] ?? string.Empty;
        var clientSecret = _configuration["Keycloak:ClientSecret"] ?? string.Empty;

        var url = $"{authority}/protocol/openid-connect/token";

        var parameters = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshTokenRequest.RefreshToken }
            };

        var content = new FormUrlEncodedContent(parameters);
        var response = await httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<JwtToken>(responseContent);
            return Ok(tokenResponse);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return Unauthorized(errorContent);
            }
            return BadRequest(errorContent);
        }
    }

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest signUpRequest)
    {
        var response = await _keycloakConfiguration.SignUp(signUpRequest);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("test-auth")]
    public IActionResult TestAuthorization()
    {
        // Only accessible to users with the "admin" role
        return Ok(new { Message = "Access granted!" });
    }
}