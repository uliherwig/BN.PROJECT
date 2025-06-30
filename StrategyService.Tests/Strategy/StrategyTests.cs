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

    private readonly IStrategyService _smaStrategy;
    private readonly IStrategyService _breakoutStrategy;
    private readonly TestLogger<SmaStrategy> _testLoggerSma;
    private readonly TestLogger<BreakoutStrategy> _testLoggerBreakout;
    private readonly IStrategyOperations _strategyOperations;
    private readonly Mock<IKafkaProducerService> _mockKafkaProducer;



    public StrategyTests()
    {
        _strategyOperations = new StrategyOperations(new TestLogger<StrategyOperations>());
        _testLoggerSma = new TestLogger<SmaStrategy>();
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

        _mockKafkaProducer = new Mock<IKafkaProducerService>();
        _mockKafkaProducer.Setup(producer => producer.SendMessageAsync("order", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

        _smaStrategy = new SmaStrategy(_testLoggerSma, _serviceProviderMock.Object, _strategyOperations, _mockKafkaProducer.Object);
        _breakoutStrategy = new BreakoutStrategy(_testLoggerBreakout, _serviceProviderMock.Object, _strategyOperations, _mockKafkaProducer.Object);
    }

    [Fact]
    public async Task RunTest_SMA_20_30_None_02_ShouldReturnTwoPositions()
    {
        // Arrange
        var testData = TestData.GetTestData();

        var count = testData.Count;
        var smaParams = JsonConvert.SerializeObject(new
        {
            ShortPeriod = 20,
            LongPeriod = 30,
            CloseType = StopLossTypeEnum.None,
            IntersectionThreshold = 0.02m
        });

        var strategySettings = new StrategySettingsModel
        {
            UserId = Guid.NewGuid(),
            StrategyType = StrategyEnum.SMA,
            Broker = "Alpaca",
            Name = "ExampleStrategy",
            Asset = "SPY",
            Quantity = 1,
            TakeProfitPercent = 0.2m,
            StopLossPercent = 0.2m,
            StartDate = DateTime.Parse("2024-11-27T14:30:00.000Z").ToUniversalTime(),
            EndDate = DateTime.Parse("2024-11-27T21:08:00.000Z").ToUniversalTime(),
            TrailingStop = 0.0m,
            AllowOvernight = true,
            Bookmarked = false,
            TestStamp = DateTime.UtcNow,
            StrategyParams = smaParams
        };

        // Act
        await _smaStrategy.StartTest(new StrategyMessage { IsBacktest = true, StrategyId = strategySettings.Id, Strategy = StrategyEnum.SMA, Settings = strategySettings });

        foreach (var quote in testData)
        {
            await _smaStrategy.EvaluateQuote(strategySettings.Id, strategySettings.UserId, quote);
        }

        // Assert
        var result = _smaStrategy.GetPositions(strategySettings.Id);

        Assert.Equal(4, result.Count);
        Assert.Equal(1, result.Count(x => x.PriceClose == 0.0m));
    }

    [Fact]
    public async Task RunTest_SMA_20_30_NextMaCrossing_01_ShouldReturnTwoPositions()
    {
        // Arrange
        var testData = TestData.GetTestData();

        var count = testData.Count;
        var smaParams = JsonConvert.SerializeObject(new
        {
            ShortPeriod = 20,
            LongPeriod = 30,
            StopLossType = StopLossTypeEnum.SMAIntersection,
            IntersectionThreshold = 0.01m
        });

        var strategySettings = new StrategySettingsModel
        {
            UserId = Guid.NewGuid(),
            StrategyType = StrategyEnum.SMA,
            Broker = "Alpaca",
            Name = "ExampleStrategy",
            Asset = "SPY",
            Quantity = 1,
            TakeProfitPercent = 0.2m,
            StopLossPercent = 0.2m,
            StartDate = DateTime.Parse("2024-11-27T14:30:00.000Z").ToUniversalTime(),
            EndDate = DateTime.Parse("2024-11-27T21:08:00.000Z").ToUniversalTime(),
            TrailingStop = 0.0m,
            AllowOvernight = true,
            Bookmarked = false,
            TestStamp = DateTime.UtcNow,
            StrategyParams = smaParams
        };

        // Act
        await _smaStrategy.StartTest(new StrategyMessage { IsBacktest = true, StrategyId = strategySettings.Id, Strategy = StrategyEnum.SMA, Settings = strategySettings });

        foreach (var quote in testData)
        {
            await _smaStrategy.EvaluateQuote(strategySettings.Id, strategySettings.UserId, quote);
        }

        // Assert
        var result = _smaStrategy.GetPositions(strategySettings.Id);

        Assert.Equal(12, result.Count);
        Assert.Equal(1, result.Count(x => x.PriceClose == 0.0m));
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
            BreakoutPeriod = BreakoutPeriodEnum.Hour
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
            AllowOvernight = true,
            Bookmarked = false,
            TestStamp = DateTime.UtcNow,
            StrategyParams = testparams
        };

        // Act
        await _breakoutStrategy.StartTest(new StrategyMessage { IsBacktest = true, StrategyId = strategySettings.Id, Strategy = StrategyEnum.Breakout, Settings = strategySettings });

        foreach (var quote in testData)
        {
            await _breakoutStrategy.EvaluateQuote(strategySettings.Id, strategySettings.UserId, quote);
        }

        // Assert
        var result = _breakoutStrategy.GetPositions(strategySettings.Id);

        Assert.Equal(3, result.Count);
        Assert.Equal(1, result.Count(x => x.PriceClose == 0.0m));
    }
    [Fact]
    public async Task RunTest_2_Tests_Parallel_ShouldReturnTwoPositions()
    {
        // Arrange
        var testData = TestData.GetTestData();

        var smaParams1 = JsonConvert.SerializeObject(new
        {
            ShortPeriod = 20,
            LongPeriod = 30,
            CloseType = StopLossTypeEnum.None,
            IntersectionThreshold = 0.02m
        });

        var strategySettings1 = new StrategySettingsModel
        {
            UserId = Guid.NewGuid(),
            StrategyType = StrategyEnum.SMA,
            Broker = "Alpaca",
            Name = "ExampleStrategy1",
            Asset = "SPY",
            Quantity = 1,
            TakeProfitPercent = 0.2m,
            StopLossPercent = 0.2m,
            StartDate = DateTime.Parse("2024-11-27T14:30:00.000Z").ToUniversalTime(),
            EndDate = DateTime.Parse("2024-11-27T21:08:00.000Z").ToUniversalTime(),
            TrailingStop = 0.0m,
            AllowOvernight = true,
            Bookmarked = false,
            TestStamp = DateTime.UtcNow,
            StrategyParams = smaParams1
        };

        var smaParams2 = JsonConvert.SerializeObject(new
        {
            ShortPeriod = 20,
            LongPeriod = 30,
            CloseType = StopLossTypeEnum.None,
            IntersectionThreshold = 0.02m
        });

        var strategySettings2 = new StrategySettingsModel
        {
            UserId = Guid.NewGuid(),
            StrategyType = StrategyEnum.SMA,
            Broker = "Alpaca",
            Name = "ExampleStrategy2",
            Asset = "SPY",
            Quantity = 1,
            TakeProfitPercent = 0.2m,
            StopLossPercent = 0.2m,
            StartDate = DateTime.Parse("2024-11-27T14:30:00.000Z").ToUniversalTime(),
            EndDate = DateTime.Parse("2024-11-27T21:08:00.000Z").ToUniversalTime(),
            TrailingStop = 0.0m,
            AllowOvernight = true,
            Bookmarked = false,
            TestStamp = DateTime.UtcNow,
            StrategyParams = smaParams2
        };

        // Act
        await _smaStrategy.StartTest(new StrategyMessage { IsBacktest = true, StrategyId = strategySettings1.Id, Strategy = StrategyEnum.SMA, Settings = strategySettings1 });
        await _smaStrategy.StartTest(new StrategyMessage { IsBacktest = true, StrategyId = strategySettings2.Id, Strategy = StrategyEnum.SMA, Settings = strategySettings2 });

        foreach (var quote in testData)
        {
            await _smaStrategy.EvaluateQuote(strategySettings1.Id, strategySettings1.UserId, quote);
            await _smaStrategy.EvaluateQuote(strategySettings2.Id, strategySettings2.UserId, quote);
        }

        // Assert
        var result1 = _smaStrategy.GetPositions(strategySettings1.Id);
        var result2 = _smaStrategy.GetPositions(strategySettings2.Id);

        Assert.Equal(4, result1.Count);
        Assert.Equal(4, result2.Count);
    }
    public async Task Demo()
    {
        var testInjection = TestInjection.Use(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Fügen Sie benutzerdefinierte Dienste hinzu
                services.AddSingleton<IStrategyOperations, StrategyOperations>();
            });

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