using System.Net.Http.Headers;

public class DataServiceClient
{
    private readonly HttpClient _httpClient;

    public DataServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri("http://localhost:9020/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<List<AlpacaAsset>?> GetAllAssetsAsync()
    {
        var response = await _httpClient.GetAsync("AlpacaDb/assets");
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var alpacaAssets = JsonConvert.DeserializeObject<List<AlpacaAsset>>(responseContent);

        return alpacaAssets;
    }

    public async Task<AlpacaAsset?> GetAssetBySymbolAsync(string symbol)
    {
        var response = await _httpClient.GetAsync($"AlpacaDb/assets/{symbol}");
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var alpacaAsset = JsonConvert.DeserializeObject<AlpacaAsset>(responseContent);

        return alpacaAsset;
    }

    public async Task AddAssetsAsync(List<AlpacaAsset> assets)
    {
        var response = await _httpClient.PostAsJsonAsync("alpacadb/assets", assets);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<AlpacaBar>?> GetHistoricalBarsAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null)
    {
        var queryParams = new List<string>();

        if (startDate != null)
            queryParams.Add($"startDate={startDate.Value.ToString("o")}");
        if (endDate != null)
            queryParams.Add($"endDate={endDate.Value.ToString("o")}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

        var response = await _httpClient.GetAsync($"AlpacaDb/bars/historical/{symbol}{queryString}");
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var alpacaBars = JsonConvert.DeserializeObject<List<AlpacaBar>>(responseContent);

        return alpacaBars;
    }

    public async Task<AlpacaBar?> GetLatestBar(string symbol)
    {

        var test = _httpClient.BaseAddress;
        var response = await _httpClient.GetAsync("AlpacaDb/bars/latest/" + symbol);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var alpacaBar = JsonConvert.DeserializeObject<AlpacaBar>(responseContent);

        return alpacaBar;
    }

    public async Task AddBarsAsync(List<AlpacaBar> bars)
    {
        var response = await _httpClient.PostAsJsonAsync("alpacadb/bars", bars);
        response.EnsureSuccessStatusCode();
    }

    public async Task CreateOrderAsync(AlpacaOrder order)
    {
        var response = await _httpClient.PostAsJsonAsync("AlpacaDb/orders", order);
        response.EnsureSuccessStatusCode();
    }

    public async Task<AlpacaOrder?> GetOrderByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"AlpacaDb/orders/{id}");
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var alpacaOrder = JsonConvert.DeserializeObject<AlpacaOrder>(responseContent);

        return alpacaOrder;
    }

    public async Task UpdateOrderAsync(int id, AlpacaOrder order)
    {
        var response = await _httpClient.PutAsJsonAsync($"AlpacaDb/orders/{id}", order);
        response.EnsureSuccessStatusCode();
    }



}
