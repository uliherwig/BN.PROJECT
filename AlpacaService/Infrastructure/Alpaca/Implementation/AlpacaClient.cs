namespace BN.PROJECT.AlpacaService;

public class AlpacaClient : IAlpacaClient
{
    private readonly IConfiguration _configuration;
    private readonly Lock _streamingClientLock = new();
    private IAlpacaDataStreamingClient? _streamingClient;

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

    // Alpaca allows only one market-data streaming connection per API key, so this
    // client must be created once and reused for every subscribe/unsubscribe call.
    public IAlpacaDataStreamingClient GetStreamingClient()
    {
        if (_streamingClient is not null)
        {
            return _streamingClient;
        }

        lock (_streamingClientLock)
        {
            if (_streamingClient is null)
            {
                var alpacaId = _configuration.GetValue<string>("Alpaca:KEY_ID") ?? string.Empty;
                var alpacaSecret = _configuration.GetValue<string>("Alpaca:SECRET_KEY") ?? string.Empty;
                _streamingClient = Alpaca.Markets.Environments.Paper.GetAlpacaDataStreamingClient(new SecretKey(alpacaId, alpacaSecret));
            }
        }

        return _streamingClient;
    }


}