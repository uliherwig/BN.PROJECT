
namespace BN.PROJECT.IdentityService;

// This attribute derives from the [Authorize] attribute, adding 
// the ability to authorize a user based on a token from Keycloak.
// The attribute uses the token to make a request to the Keycloak
// userinfo endpoint to verify the token's validity.


public class KeycloakAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
{
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

        var url = $"{authority}/protocol/openid-connect/userinfo";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
