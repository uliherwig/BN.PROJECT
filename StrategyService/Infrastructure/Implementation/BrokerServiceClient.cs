using System.Configuration;

namespace BN.PROJECT.StrategyService;

public class BrokerServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public BrokerServiceClient(HttpClient httpClient, 
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri(_configuration["AlpacaServiceClient"]);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<string> GetStrategyAsync()
    {
        var response = await _httpClient.GetAsync($"/strategy");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> StartStrategyAsync(StrategySettingsModel testSettings)
    {
        var json = JsonConvert.SerializeObject(testSettings);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"/strategy", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}