namespace BN.PROJECT.IdentityService;

public class KeycloakServiceClient : IKeycloakServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakServiceClient> _logger;

    private readonly string _authority;
    private readonly string _realm;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public KeycloakServiceClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<KeycloakServiceClient> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri(_configuration["Keycloak:Host"]);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _authority = _configuration["Keycloak:Authority"] ?? string.Empty;
        _realm = _configuration["Keycloak:Realm"] ?? string.Empty;
        _clientId = _configuration["Keycloak:ClientId"] ?? string.Empty;
        _clientSecret = _configuration["Keycloak:ClientSecret"] ?? string.Empty;

        _logger.LogInformation("KeycloakServiceClient initialized.");
        _logger.LogInformation($"Authority: {_authority}");
        _logger.LogInformation($"Realm: {_realm}");
        _logger.LogInformation($"ClientId: {_clientId}");
        _logger.LogInformation($"ClientSecret: {_clientSecret}");
        _logger.LogInformation($"Host: {_configuration["Keycloak:Host"]}");
    }

    public async Task<SignInResponse> SignIn(SignInRequest signInRequest)
    {      

        if (signInRequest == null || string.IsNullOrEmpty(signInRequest.Username) || string.IsNullOrEmpty(signInRequest.Password))
        {
            return new SignInResponse
            {
                Success = false,
                ErrorCode = AuthErrorCode.InvalidCredentials,
                JwtToken = null
            };
        }

        var endpoint = $"{_authority}/protocol/openid-connect/token";

        var parameters = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "password" },
                { "username", signInRequest.Username },
                { "password", signInRequest.Password }
            };

        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(endpoint, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<JwtToken>(responseContent);
            var claims = JwtTokenDecoder.DecodeJwtToken(tokenResponse.AccessToken);

            tokenResponse.Name = claims["preferred_username"];
            return new SignInResponse
            {
                Success = true,
                ErrorCode = AuthErrorCode.None,
                JwtToken = tokenResponse
            };
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new SignInResponse
                {
                    Success = false,
                    ErrorCode = AuthErrorCode.Unauthorized,
                    JwtToken = null
                };
            }
            return new SignInResponse
            {
                Success = false,
                ErrorCode = AuthErrorCode.InternalServerError,
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

        var endpoint = $"{_authority}/protocol/openid-connect/logout";

        var parameters = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "refresh_token", signOutRequest.RefreshToken }
            };

        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(endpoint, content);

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

    public async Task<SignUpResponse> SignUp(SignUpRequest registerRequest)

    {
        var signUpResponse = new SignUpResponse();
        var adminToken = await GetAdminAccessToken();

        var endpoint = $"/admin/realms/{_realm}/users";

        var user = new
        {
            username = registerRequest.Email,
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
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(request);
        signUpResponse.Success = response.IsSuccessStatusCode;

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            switch (response.StatusCode)
            {  
                case HttpStatusCode.Conflict:
                    // Common when user/email already exists
                    signUpResponse.ErrorCode = AuthErrorCode.EmailAlreadyExists;
                    _logger.LogError("User/email already exists: {Error}", errorContent);
                    break;        

                default:
                    signUpResponse.ErrorCode = AuthErrorCode.InternalServerError;
                    _logger.LogError("Unexpected error creating user (Status: {Status}): {Error}", response.StatusCode, errorContent);
                    break;
            }

            return signUpResponse;
        }

        var locationHeader = response.Headers.Location;
        signUpResponse.UserId = locationHeader.Segments.Last();

        await AddUserRole(adminToken, signUpResponse.UserId);

        return signUpResponse;
    }

    public async Task<JwtToken> RefreshToken(RefreshTokenRequest refreshTokenRequest)
    {      
      
        var endpoint = $"{_authority}/protocol/openid-connect/token";

        var parameters = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshTokenRequest.RefreshToken }
            };

        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(endpoint, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<JwtToken>(responseContent);
            return tokenResponse;
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(errorContent);
            }
            return null;
            //throw new Exception(errorContent);
        }
    }

    private async Task AddUserRole(string adminToken, string userId)
    {
        var userRole = await GetRoleByName(adminToken, "user");

        var endpoint = $"/admin/realms/{_realm}/users/{userId}/role-mappings/realm";

        var roles = new[] {
            new
            {
                id = userRole.Id,
                name = userRole.Name
            }
        };

        var roleContent = new StringContent(JsonConvert.SerializeObject(roles), Encoding.UTF8, "application/json");
        var roleRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = roleContent
        };
        roleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest);
        if (!roleResponse.IsSuccessStatusCode)
        {
            var roleErrorContent = await roleResponse.Content.ReadAsStringAsync();
            _logger.LogError(roleErrorContent);
        }
    }

    public async Task<User> GetUserByName(string username)
    {
        var endpoint = $"/admin/realms/{_realm}/users?username={username}";
        var adminToken = await GetAdminAccessToken();

        var userRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var userResponse = await _httpClient.SendAsync(userRequest);
        if (!userResponse.IsSuccessStatusCode)
        {
            var roleErrorContent = await userResponse.Content.ReadAsStringAsync();
            _logger.LogError(roleErrorContent);
        }
        if (userResponse.IsSuccessStatusCode)
        {
            var responseContent = await userResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
            var unixStamp = (long)result[0].createdTimestamp;
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixStamp / 1000).UtcDateTime;

            var user = new User
            {
                UserId = result[0].id,
                Username = result[0].username,
                Email = result[0].email,
                FirstName = result[0].firstName,
                LastName = result[0].lastName,
                CreatedAt = dateTime,
                UpdatedAt = dateTime,
                IsEmailVerified = false
            };
            return user;
        }
        else
        {
            throw new Exception("Unable to retrieve user.");
        }
    }

    private async Task<Role> GetRoleByName(string adminToken, string rolename)
    {
        var endpoint = $"/admin/realms/{_realm}/roles/{rolename}";

        var rolesRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        rolesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var rolesResponse = await _httpClient.SendAsync(rolesRequest);
        var responseContent = await rolesResponse.Content.ReadAsStringAsync();
        if (!rolesResponse.IsSuccessStatusCode)
        {
            _logger.LogError(responseContent);
            throw new Exception("Unable to retrieve role.");
        }
        return JsonConvert.DeserializeObject<Role>(responseContent);
    }

    private async Task<string> GetAdminAccessToken()
    {
        var endpoint = $"{_authority}/protocol/openid-connect/token";

        var parameters = new Dictionary<string, string>
            {
                { "client_id", _configuration["Keycloak:AdminClientId"] },
                { "client_secret",_configuration["Keycloak:AdminClientSecret"] },
                { "grant_type", "client_credentials" }
            };

        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(endpoint, content);

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

    public async Task<SignOutResponse> DeleteUser(string userId)
    {
        var signOutResponse = new SignOutResponse();

        var adminToken = await GetAdminAccessToken();
        var endpoint = $"/admin/realms/{_realm}/users/{userId}";
        var userRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var userResponse = await _httpClient.SendAsync(userRequest);
        signOutResponse.Success = userResponse.IsSuccessStatusCode;
        if (!userResponse.IsSuccessStatusCode)
        {
            var roleErrorContent = await userResponse.Content.ReadAsStringAsync();
            _logger.LogError(roleErrorContent);
            signOutResponse.Errors = roleErrorContent;
        }
        return signOutResponse;
    }

    public async Task<bool> UserExistsById(string userId)
    {
        var endpoint = $"/admin/realms/{_realm}/users/{userId}";
        var adminToken = await GetAdminAccessToken();

        var userRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var userResponse = await _httpClient.SendAsync(userRequest);
        return userResponse.IsSuccessStatusCode;
    }

    // Set Email as verified
    public async Task<bool> VerifyEmail(User user)
    {
        var getUserEndpoint = $"/admin/realms/{_realm}/users?username={user.Username}";
        var adminToken = await GetAdminAccessToken();

        var userRequest = new HttpRequestMessage(HttpMethod.Get, getUserEndpoint);
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var userResponse = await _httpClient.SendAsync(userRequest);
        if (!userResponse.IsSuccessStatusCode)
        {
            var roleErrorContent = await userResponse.Content.ReadAsStringAsync();
            _logger.LogError(roleErrorContent);
            return false;
        }
        if (userResponse.IsSuccessStatusCode)
        {
            var responseContent = await userResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
            var endpoint = $"/admin/realms/{_realm}/users/{user.UserId}";

            var userUpdate = new
            {             
                username = result[0].username,
                email = result[0].email,
                emailVerified = true
            };

            var content = new StringContent(JsonConvert.SerializeObject(userUpdate), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        return false;
    }
}