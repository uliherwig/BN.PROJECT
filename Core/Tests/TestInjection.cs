// <copyright company="BSH Hausgeräte GmbH"></copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace BN.PROJECT.Core;

public class TestInjection
{
    private readonly Action<IWebHostBuilder> _configureWebHost;

    public TestInjection(Action<IWebHostBuilder> configureWebHost)
    {
        _configureWebHost = configureWebHost;
    }

    public static TestInjection Use(Action<IWebHostBuilder> configureWebHost) => new(configureWebHost);

    public static TestInjection UseDefault() => Use(DefaultConfigureWebHost);

    public void Run()
    {
        var builder = WebApplication.CreateBuilder();
        _configureWebHost(builder.WebHost);
        var app = builder.Build();
        ConfigureApp(app);
        app.Run();
    }

    private static void DefaultConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        });
    }

    private static void ConfigureApp(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
    }
}