namespace BN.PROJECT.IdentityService
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

        public async Task<SignInResponse> SignIn(SignInRequest signInRequest)
        {
            var signInResponse = new SignInResponse
            {
                Success = false,
                Errors = "Invalid username or password.",
                JwtToken = null
            };

            if (signInRequest == null || string.IsNullOrEmpty(signInRequest.Username) || string.IsNullOrEmpty(signInRequest.Password))
            {
                return signInResponse;
            }
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
                { "username", signInRequest.Username },
                { "password", signInRequest.Password }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<JwtToken>(responseContent);
                var claims = JwtTokenDecoder.DecodeJwtToken(tokenResponse.AccessToken);

                // TODO : Create a User class to store user information in db 
                var user = new User
                {
                    Id = claims["sub"],
                    Name = claims["preferred_username"],
                    Email = claims["email"],
                    FirstName = claims["given_name"],
                    LastName = claims["family_name"]
                };
                tokenResponse.Name = user.Name;
                return new SignInResponse
                {
                    Success = true,
                    Errors = string.Empty,
                    JwtToken = tokenResponse
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return signInResponse;
                }
                return new SignInResponse
                {
                    Success = false,
                    Errors = errorContent,
                    JwtToken = null
                };
            }
        }

        public async Task<SignOutResponse> SignOut(SignOutRequest signOutRequest)
        {
            var signOutResponse = new SignOutResponse
            {
                Success = false,
                Errors = string.Empty
            };
            var httpClient = _httpClientFactory.CreateClient();

            var authority = _configuration["Keycloak:Authority"] ?? string.Empty;
            var clientId = _configuration["Keycloak:ClientId"] ?? string.Empty;
            var clientSecret = _configuration["Keycloak:ClientSecret"] ?? string.Empty;

            var url = $"{authority}/protocol/openid-connect/logout";

            var parameters = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "refresh_token", signOutRequest.RefreshToken }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                signOutResponse.Success = true;
                return signOutResponse;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                signOutResponse.Errors = errorContent;
                return signOutResponse;
            }
        }

        public async Task<SignInResponse> SignUp(SignUpRequest registerRequest)
        {
            var signUpResponse = new SignInResponse();
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
            signUpResponse.Success = response.IsSuccessStatusCode;

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                signUpResponse.Errors = errorContent;
                _logger.LogError(errorContent);
            }
            return signUpResponse;
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
                var tokenResponse = JsonConvert.DeserializeObject<JwtToken>(responseContent);
                return tokenResponse.AccessToken;
            }
            else
            {
                throw new Exception("Unable to retrieve admin access token.");
            }
        }

    }
}