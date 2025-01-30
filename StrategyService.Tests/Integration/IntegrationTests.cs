using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BN.PROJECT.StrategyService.Tests;

public class IntegrationTests
{
    private readonly HttpClient _client;

    public IntegrationTests()
    {
        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                // Add TestServer
                webHost.UseTestServer();

                webHost.ConfigureServices(services =>
                {
                    // Replace real services with mock services
                });

                webHost.Configure(app =>
                {
                    app.Run(async ctx =>
                    {
                        await ctx.Response.WriteAsync("Hello World!");
                    });
                });
            });

        // Build and start the IHost
        var host = hostBuilder.StartAsync().Result;

        // Create an HttpClient to send requests to the TestServer
        _client = host.GetTestClient();
    }

    [Fact]
    public async Task ShouldReturnHelloWorld()
    {
        var response = await _client.GetAsync("/");

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello World!", responseString);
    }
}