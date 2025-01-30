namespace BN.PROJECT.AlpacaService;

public class StrategyServiceClient : IStrategyServiceClient
{
    private readonly HttpClient _httpClient;

    public StrategyServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5101");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<StrategySettingsModel?> GetStrategyAsync(string strategyId)
    {
        var response = await _httpClient.GetAsync($"/strategy/{strategyId}");
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
            var response = await _httpClient.PostAsync($"/strategy", content);

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}