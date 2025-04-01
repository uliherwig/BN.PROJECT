namespace BN.PROJECT.AlpacaService;

public class StrategyServiceClient : IStrategyServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public StrategyServiceClient(HttpClient httpClient, 
        IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_configuration["StrategyServiceClient"]); 
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
     
    }

    public async Task<StrategySettingsModel?> GetStrategyAsync(string strategyId)
    {
        var response = await _httpClient.GetAsync($"/internalstrategy/{strategyId}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();

        var settings = JsonConvert.DeserializeObject<StrategySettingsModel>(result);
        return settings;
    }

    public async Task<string> StartStrategyAsync(StrategySettingsModel testSettings)
    {
        try
        {
            var json = JsonConvert.SerializeObject(testSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"/internalstrategy", content);

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}