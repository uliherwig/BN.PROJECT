namespace BN.PROJECT.IdentityService;


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

        //  postions strategies
    }

    public async Task RemoveUserData(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/strategy/remove-user-data");
      
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        await response.Content.ReadAsStringAsync();      
    }
}