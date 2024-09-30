namespace BN.PROJECT.AlpacaService;
public class StrategyServiceClient : IStrategyServiceClient
{
    private readonly HttpClient _httpClient;

    public StrategyServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://localhost:7167");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<string> GetStrategyAsync()
    {
        var response = await _httpClient.GetAsync($"/strategy");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    public async Task<string> StartStrategyAsync(BacktestSettings testSettings)
    {
        var json = JsonConvert.SerializeObject(testSettings);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"/strategy", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
