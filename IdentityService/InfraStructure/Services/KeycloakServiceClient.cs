using Azure;
using IdentityModel.Client;
using NuGet.Packaging.Signing;

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
            signUpResponse.Errors = errorContent;
            _logger.LogError(errorContent);
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
            throw new Exception(errorContent);
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

            var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixStamp/1000).UtcDateTime;

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

}
