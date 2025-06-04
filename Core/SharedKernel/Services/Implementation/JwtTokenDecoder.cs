namespace BN.PROJECT.Core;

public static class JwtTokenDecoder 
{
    public static IDictionary<string, string> DecodeJwtToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

        if (jsonToken == null)
        {
            throw new ArgumentException("Invalid JWT token");
        }

        var claims = new Dictionary<string, string>();
        foreach (var item in jsonToken.Claims)
        {
            claims.TryAdd(item.Type, item.Value);
        }
        return claims;
    }

    public static string GetClaimValue(string token, string claimType)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        if (jsonToken == null)
        {
            throw new ArgumentException("Invalid JWT token");
        }
        var claim = jsonToken.Claims.FirstOrDefault(c => c.Type == claimType);
        return claim?.Value;


        //var authorizationHeader = Request.Headers["Authorization"].ToString();
        //if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        //{
        //    return Unauthorized();
        //}

        //var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        //var claims = JwtTokenDecoder.DecodeJwtToken(token);
        //if (claims == null || !claims.ContainsKey("sub"))
        //{
        //    return Unauthorized();
        //}

        //var userId = new Guid(claims["sub"]);
    }
}