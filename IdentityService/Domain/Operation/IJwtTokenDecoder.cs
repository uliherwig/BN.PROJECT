namespace BN.PROJECT.IdentityService;

public interface IJwtTokenDecoder
{
    IDictionary<string, string> DecodeJwtToken(string token);
}