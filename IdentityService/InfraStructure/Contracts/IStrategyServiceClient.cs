namespace BN.PROJECT.IdentityService;

public interface IStrategyServiceClient
{
    Task RemoveUserData(string userId);
}