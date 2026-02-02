using static System.Net.Mime.MediaTypeNames;

namespace BN.PROJECT.AlpacaService;

public class FinAIServiceClient : IFinAIServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public FinAIServiceClient(
        HttpClient httpClient, 
        IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_configuration["FinAIServiceClient"]); 
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
     
    }

    public async Task<string?> TestOptimizationAsync()
    {
        var response = await _httpClient.GetAsync($"/api/v1/hello");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();    
        return result;
    }

    public async Task<string?> CreateDataframeAsync(StrategySettingsModel testSettings)
    {
        try
        {
            var json = JsonConvert.SerializeObject(testSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            

            if(testSettings.StrategyType == StrategyEnum.IndicatorBased)
            {
                var response = await _httpClient.PostAsync($"/api/v1/indicator-test", content);
                return await response.Content.ReadAsStringAsync();
            }
            if (testSettings.StrategyType == StrategyEnum.MachineLearningBased)
            {
                var response = await _httpClient.PostAsync($"/api/v1/get-lgb-dataframe", content);
                return await response.Content.ReadAsStringAsync();               
            }

            return null;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    public async Task<string> StartOptimizerAsync(StrategySettingsModel testSettings)
    {
        try
        {
            var json = JsonConvert.SerializeObject(testSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"/api/v1/optimize-strategy", content);

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

}