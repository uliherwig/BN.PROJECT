
namespace BN.PROJECT.Core;
public class KeycloakAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public KeycloakAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }
    public async void OnAuthorization(AuthorizationFilterContext context)
    {
        var httpClientFactory = context.HttpContext.RequestServices.GetService<IHttpClientFactory>();
        var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();

        if (httpClientFactory == null || configuration == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var httpClient = httpClientFactory.CreateClient();
        var authority = configuration["Keycloak:Authority"] ?? string.Empty;
        var token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        var claims = JwtTokenDecoder.DecodeJwtToken(token);
        var realmAccess = claims["realm_access"];
        bool hasRequiredRole = _roles.Any(role => realmAccess.Contains(role));

        if (!hasRequiredRole)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
