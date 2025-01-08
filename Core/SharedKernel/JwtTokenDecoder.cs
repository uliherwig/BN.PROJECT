namespace BN.PROJECT.Core;

public class JwtTokenDecoder
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
}