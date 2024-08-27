namespace BN.TRADER.IdentityService
{
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
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
                { "grant_type", "password" },
                { "username", loginRequest.Username },
                { "password", loginRequest.Password }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<JwtTokenResponse>(responseContent);
                return Ok(tokenResponse);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return BadRequest(errorContent);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            var isRegistrationSuccess = await _keycloakConfiguration.Register(registerRequest);

            if (isRegistrationSuccess)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Registration not successful");
            }
        }

        [Authorize]
        [HttpGet("test-auth")]
        public IActionResult TestAuthorization()
        {
            // Only accessible to users with the "admin" role
            return Ok(new { Message = "Admin access granted!" });
        }    


    }
}
