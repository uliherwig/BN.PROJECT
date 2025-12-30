using BN.PROJECT.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;

namespace BN.PROJECT.StrategyService.Tests;

public class StrategyTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IStrategyRepository> _strategyRepositoryServiceMock;
    private readonly IStrategyService _breakoutStrategy;

    private readonly TestLogger<BreakoutStrategy> _testLoggerBreakout;
    private readonly Mock<IRedisPublisher> _mockRedisPublisher;

    public StrategyTests()
    {
    
        _testLoggerBreakout = new TestLogger<BreakoutStrategy>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _strategyRepositoryServiceMock = new Mock<IStrategyRepository>();

        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_serviceScopeFactoryMock.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);

        _serviceProviderMock.Setup(x => x.GetService(typeof(IStrategyRepository))).Returns(_strategyRepositoryServiceMock.Object);
        _strategyRepositoryServiceMock.Setup(x => x.AddPositionsAsync(It.IsAny<List<PositionModel>>())).Returns(Task.CompletedTask);
        _mockRedisPublisher = new Mock<IRedisPublisher>();
        _mockRedisPublisher.Setup(publisher => publisher.Publish(It.IsAny<string>(), It.IsAny<string>()))
                             .Returns(Task.CompletedTask);
   
        _breakoutStrategy = new BreakoutStrategy(_testLoggerBreakout,  _mockRedisPublisher.Object);
    }

    [Fact]
    public async Task RunTest_Breakout_Day_ShouldReturnTwoPositions()
    {
        // Arrange
        var testData = TestData.GetTestData();

        var count = testData.Count;
        var testparams = JsonConvert.SerializeObject(new
        {
            StopLossType = StopLossTypeEnum.Breakout,
            BreakoutPeriod = TimeFrameEnum.Hour
        });

        var strategySettings = new StrategySettingsModel
        {
            UserId = Guid.NewGuid(),
            StrategyType = StrategyEnum.Breakout,
            Broker = "Alpaca",
            Name = "ExampleStrategy",
            Asset = "SPY",
            Quantity = 1,
            TakeProfitPercent = 0.2m,
            StopLossPercent = 0.2m,
            StartDate = DateTime.Parse("2024-11-27T14:30:00.000Z").ToUniversalTime(),
            EndDate = DateTime.Parse("2024-11-27T21:08:00.000Z").ToUniversalTime(),
            TrailingStop = 0.0m,
            ClosePositionEod = true,
            Bookmarked = false,
            StrategyParams = testparams
        };

        // Act
        await _breakoutStrategy.StartTest(new StrategyMessage { IsBacktest = true, StrategyId = strategySettings.Id, Strategy = StrategyEnum.Breakout, Settings = strategySettings });

        foreach (var quote in testData)
        {
            await _breakoutStrategy.EvaluateQuote(strategySettings.Id, strategySettings.UserId, quote);
        }

        // Assert
        var result = _breakoutStrategy.GetPositions();

        Assert.Equal(3, result.Count);
        Assert.Equal(1, result.Count(x => x.PriceClose == 0.0m));
    }
    public async Task Demo()
    {
        var testInjection = TestInjection.Use(builder =>
        { 

            builder.Configure(app =>
            {
                // Konfigurieren Sie die Middleware-Pipeline
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });
        });

        testInjection.Run();
    }
}