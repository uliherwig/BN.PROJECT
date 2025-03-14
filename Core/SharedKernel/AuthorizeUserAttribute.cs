namespace BN.PROJECT.Core;

public class AuthorizeUserAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public AuthorizeUserAttribute(params string[] roles)
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
        var httpClient = httpClientFactory.CreateClient();

        // check if the request has an Authorization header
        var authorizationHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // check if the token is valid
        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        var claims = JwtTokenDecoder.DecodeJwtToken(token);
        if (claims == null || !claims.ContainsKey("sub"))
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        var userId = claims["sub"];

        // check if the user has the required role
        var realmAccess = claims["realm_access"];
        bool hasRequiredRole = _roles.Any(role => realmAccess.Contains(role));
        if (!hasRequiredRole)
        {
            context.Result = new UnauthorizedResult();
            return;
        }   

        // add the user id and token to the context 
        context.HttpContext.Items["UserId"] = userId;
        context.HttpContext.Items["token"] = token;
    }

  
}
