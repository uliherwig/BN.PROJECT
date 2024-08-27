namespace BN.TRADER.IdentityService
{
    public interface IKeycloakService
    {
        Task<bool> Register(RegisterRequest registerRequest);
    }
}
