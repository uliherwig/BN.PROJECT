namespace BN.TRADER.IdentityService
{
    public class KeycloakService : IKeycloakService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KeycloakService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public KeycloakService(
            IConfiguration
            configuration, ILogger<KeycloakService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private async Task<string> GetAdminAccessTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["Keycloak:Authority"]}/protocol/openid-connect/token";

            var parameters = new Dictionary<string, string>
            {
                { "client_id", _configuration["Keycloak:AdminClientId"] },
                { "client_secret",_configuration["Keycloak:AdminClientSecret"] },
                { "grant_type", "client_credentials" }
            };        

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<JwtTokenResponse>(responseContent);
                return tokenResponse.AccessToken;
            }
            else
            {
                throw new Exception("Unable to retrieve admin access token.");
            }
        }

        [HttpPost("register")]
        public async Task<bool> Register(RegisterRequest registerRequest)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var adminToken = await GetAdminAccessTokenAsync();

            var url = $"{_configuration["Keycloak:Host"]}/admin/realms/{_configuration["Keycloak:Realm"]}/users";

            var user = new
            {
                username = registerRequest.Username,
                email = registerRequest.Email,
                firstName = registerRequest.FirstName,
                lastName = registerRequest.LastName,
                enabled = true,
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = registerRequest.Password,
                        temporary = false
                    }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(errorContent);
            }
            return response.IsSuccessStatusCode;
        }

    }
}