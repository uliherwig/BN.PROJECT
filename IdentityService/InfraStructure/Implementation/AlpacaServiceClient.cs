using NuGet.Common;

namespace BN.PROJECT.IdentityService;


public class AlpacaServiceClient : IAlpacaServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AlpacaServiceClient(HttpClient httpClient,
        IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_configuration["AlpacaServiceClient"]);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // usersettings, positions, orders, executions
    }

    public async Task DeleteUserSettings(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/usersettings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        await response.Content.ReadAsStringAsync();
    }

    // TODO : Implement the following methods if needed

    //public async Task DeletePositions(string userId)
    //{
    //    var request = new HttpRequestMessage(HttpMethod.Delete, $"/usersettings/{userId}");
    //    var response = await _httpClient.SendAsync(request);
    //    response.EnsureSuccessStatusCode();
    //    await response.Content.ReadAsStringAsync();
    //}

    //public async Task DeleteOrders(string userId)
    //{
    //    var request = new HttpRequestMessage(HttpMethod.Delete, $"/usersettings/{userId}");
    //    var response = await _httpClient.SendAsync(request);
    //    response.EnsureSuccessStatusCode();
    //    await response.Content.ReadAsStringAsync();
    //}

    public async Task DeleteExecutions(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/alpacatrading/delete-executions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        await response.Content.ReadAsStringAsync();
    }
}