using BN.PROJECT.Core;

public class AuthorizeIntrospectAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public AuthorizeIntrospectAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public async void OnAuthorization(AuthorizationFilterContext context)
    {
        var httpClientFactory = context.HttpContext.RequestServices.GetService<IHttpClientFactory>();
        var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
        var authorizationService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();

        if (httpClientFactory == null || configuration == null || authorizationService == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var authorizationHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        if (!await IntrospectTokenAsync(context, token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var claims = JwtTokenDecoder.DecodeJwtToken(token);
        if (claims == null || !claims.ContainsKey("sub"))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = claims["sub"];
        var realmAccess = claims["realm_access"];
        bool hasRequiredRole = _roles.Any(role => realmAccess.Contains(role));
        if (!hasRequiredRole)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        context.HttpContext.Items["UserId"] = userId;
        context.HttpContext.Items["token"] = token;
    }

    private async Task<bool> IntrospectTokenAsync(AuthorizationFilterContext context, string token)
    {
        var httpClientFactory = context.HttpContext.RequestServices.GetService<IHttpClientFactory>();
        var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();

        if (httpClientFactory == null || configuration == null)
        {
            return false;
        }

        var httpClient = httpClientFactory.CreateClient();
        var authority = configuration["Keycloak:Authority"] ?? string.Empty;
        var realm = configuration["Keycloak:Realm"] ?? string.Empty;
        var clientId = configuration["Keycloak:ClientId"] ?? string.Empty;
        var clientSecret = configuration["Keycloak:ClientSecret"] ?? string.Empty;

        var request = new HttpRequestMessage(HttpMethod.Post, $"{authority}/realms/{realm}/protocol/openid-connect/token/introspect");
        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("token", token)
        });

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var json = await response.Content.ReadAsStringAsync();
        return true;
        //var tokenInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        //return tokenInfo != null && tokenInfo.ContainsKey("active") && (bool)tokenInfo["active"];
    }
}
