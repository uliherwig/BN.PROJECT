namespace BN.PROJECT.AlpacaService;

public class AlpacaClient : IAlpacaClient
{
    private readonly IConfiguration _configuration;

    public AlpacaClient(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IAlpacaDataClient GetAlpacaDataClient()
    {
        var alpacaId = _configuration.GetValue<string>("Alpaca:KEY_ID") ?? string.Empty;
        var alpacaSecret = _configuration.GetValue<string>("Alpaca:SECRET_KEY") ?? string.Empty;
        return Alpaca.Markets.Environments.Paper.GetAlpacaDataClient(new SecretKey(alpacaId, alpacaSecret));
    }

    public IAlpacaTradingClient GetCommonTradingClient()
    {
        var alpacaId = _configuration.GetValue<string>("Alpaca:KEY_ID") ?? string.Empty;
        var alpacaSecret = _configuration.GetValue<string>("Alpaca:SECRET_KEY") ?? string.Empty;
        return Alpaca.Markets.Environments.Paper.GetAlpacaTradingClient(new SecretKey(alpacaId, alpacaSecret));
    }

    public IAlpacaTradingClient? GetPrivateTradingClient(UserSettingsModel userSettings)
    {
        var alpacaId = userSettings.AlpacaKey;
        var alpacaSecret = userSettings.AlpacaSecret;
        return Alpaca.Markets.Environments.Paper.GetAlpacaTradingClient(new SecretKey(alpacaId, alpacaSecret));
    }
}