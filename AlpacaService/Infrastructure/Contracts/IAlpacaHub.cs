
namespace BN.PROJECT.AlpacaService
{
    public interface IAlpacaHub
    {
        Task<string> GetConnectinId(string userId);
    }
}