namespace BN.PROJECT.AlpacaService;

public interface IStartUpService
{
    Task InitializeTradesStorage(List<string> assetsSelection);
}
