using Microsoft.AspNetCore.Hosting;

namespace BN.PROJECT.Core;

public class TestWebHostBuilder : IWebHostBuilder
{
    private readonly WebHostBuilder _webHostBuilder;

    public IWebHost WebHost { get; private set; }

    public bool ExecuteInit { get; set; }

    public TestWebHostBuilder()
    {
        _webHostBuilder = new WebHostBuilder();
    }

    public IWebHost Build()
    {
        if (WebHost != null)
        {
            return WebHost;
        }

        return _webHostBuilder.Build();
    }

    public IWebHostBuilder ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        _webHostBuilder.ConfigureAppConfiguration(configureDelegate);
        return this;
    }

    public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
    {
        _webHostBuilder.ConfigureServices(configureServices);
        return this;
    }

    public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
    {
        _webHostBuilder.ConfigureServices(configureServices);
        return this;
    }

    public string GetSetting(string key)
    {
        return _webHostBuilder.GetSetting(key);
    }

    public IWebHostBuilder UseSetting(string key, string value)
    {
        _webHostBuilder.UseSetting(key, value);
        return this;
    }
}