namespace BN.PROJECT.AlpacaService;

public interface IAlpacaClient
{
    IAlpacaDataClient GetAlpacaDataClient();

    IAlpacaTradingClient GetCommonTradingClient();

    IAlpacaTradingClient? GetPrivateTradingClient(UserSettingsModel userSettings);
}