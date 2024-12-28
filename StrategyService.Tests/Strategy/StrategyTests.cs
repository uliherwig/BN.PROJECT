using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using BN.PROJECT.StrategyService;
using BN.PROJECT.Core;
using System.Text.Json;
using Moq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace BN.PROJECT.StrategyService.Tests
{
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
            _strategyRepositoryServiceMock.Setup(x => x.AddPositionsAsync(It.IsAny<List<Position>>())).Returns(Task.CompletedTask);

            _smaStrategy = new SmaStrategy(_testLoggerSma, _serviceProviderMock.Object, _strategyOperations);
            _breakoutStrategy = new BreakoutStrategy(_testLoggerBreakout, _serviceProviderMock.Object, _strategyOperations);
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
            await _smaStrategy.StartTest(new StrategyMessage { StrategyId = strategySettings.Id, Strategy = StrategyEnum.SMA, Settings = strategySettings });

            // Act
            foreach (var quote in testData)
            {
                await _smaStrategy.EvaluateQuote(strategySettings.Id, quote);
            }

            // Assert
            var result = _smaStrategy.GetPositions(strategySettings.Id);

            Assert.Equal(2, result.Count);
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
                StopLossType = StopLossTypeEnum.SMANextCrossing,
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
            await _smaStrategy.StartTest(new StrategyMessage { StrategyId = strategySettings.Id, Strategy = StrategyEnum.SMA, Settings = strategySettings });

            // Act
            foreach (var quote in testData)
            {
                await _smaStrategy.EvaluateQuote(strategySettings.Id, quote);
            }

            // Assert
            var result = _smaStrategy.GetPositions(strategySettings.Id);

            Assert.Equal(6, result.Count);
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
                BreakoutPeriod = BreakoutPeriodEnum.Hour            });


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
            await _breakoutStrategy.StartTest(new StrategyMessage { StrategyId = strategySettings.Id, Strategy = StrategyEnum.Breakout, Settings = strategySettings });

            // Act
            foreach (var quote in testData)
            {
                await _breakoutStrategy.EvaluateQuote(strategySettings.Id, quote);
            }

            // Assert
            var result = _breakoutStrategy.GetPositions(strategySettings.Id);

            Assert.Equal(3, result.Count);
            Assert.Equal(1, result.Count(x => x.PriceClose == 0.0m));
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
}
