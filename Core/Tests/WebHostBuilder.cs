// <copyright company="BSH Hausgeräte GmbH"></copyright>

using Microsoft.AspNetCore.Hosting;

namespace BSH.MES.Testing.Common;

/// <summary>
/// Necessary when Bootstrapper calls the build Method to intercept the instance
/// </summary>
public class TestWebHostBuilder : IWebHostBuilder
{
    private readonly WebHostBuilder _webHostBuilder;
    public IWebHost WebHost { get; private set; }

    public TestWebHostBuilder()
    {
        _webHostBuilder = new WebHostBuilder();
    }

    public bool ExecuteInit { get; set; }

    public IWebHost Build()
    {
        if (WebHost != null)
        {
            return WebHost;
        }

        WebHost = _webHostBuilder.Build();

        return WebHost;
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

    public string GetSetting(string key) => _webHostBuilder.GetSetting(key);

    public IWebHostBuilder UseSetting(string key, string value)
    {
        _webHostBuilder.UseSetting(key, value);
        return this;
    }
}