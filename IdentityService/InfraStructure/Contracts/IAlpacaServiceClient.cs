
namespace BN.PROJECT.IdentityService;

public interface IAlpacaServiceClient
{
    Task DeleteExecutions(string userId);
    Task DeleteUserSettings(string userId);
}