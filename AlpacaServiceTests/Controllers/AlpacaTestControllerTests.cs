using BN.PROJECT.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BN.PROJECT.AlpacaService.Tests
{
    public class AlpacaTestControllerTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<IAlpacaRepository> _mockAlpacaRepository;
        private readonly Mock<IStrategyTestService> _mockBacktestService;
        private readonly Mock<IStrategyServiceClient> _mockStrategyServiceClient;
        private readonly Mock<IOptimizerServiceClient> _mockOptimizerServiceClient;
        private readonly Mock<ILogger<AlpacaTestController>> _mockLogger;
        private readonly AlpacaTestController _alpacaTestController;

        public AlpacaTestControllerTests()
        {
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockAlpacaRepository = new Mock<IAlpacaRepository>();
            _mockBacktestService = new Mock<IStrategyTestService>();
            _mockStrategyServiceClient = new Mock<IStrategyServiceClient>();
            _mockOptimizerServiceClient = new Mock<IOptimizerServiceClient>();
            _mockLogger = new Mock<ILogger<AlpacaTestController>>();
            _alpacaTestController = new AlpacaTestController(_mockEnv.Object,
                                              _mockAlpacaRepository.Object,
                                              _mockBacktestService.Object,
                                              _mockStrategyServiceClient.Object,
                                               _mockOptimizerServiceClient.Object,
                                              _mockLogger.Object);
        }

        [Fact]
        public async Task GetHistoricalBarsBySymbol_ShouldReturnOkWithBars()
        {
            // Arrange
            var symbol = "AAPL";
            var startDate = new DateTime(2023, 1, 1).ToUniversalTime();
            var endDate = new DateTime(2023, 1, 2).ToUniversalTime();
            var mockBars = new List<AlpacaBar>
            {
                new() { T = startDate, O = 150, H = 155, L = 149, C = 152, V = 1000, Symbol = symbol },
                new() { T = endDate, O = 152, H = 156, L = 151, C = 154, V = 1200, Symbol = symbol }
            };

            _mockAlpacaRepository.Setup(repo => repo.GetHistoricalBars(symbol, startDate.ToUniversalTime(), endDate.ToUniversalTime()))
                .ReturnsAsync(mockBars);

            // Act
            var result = await _alpacaTestController.GetHistoricalBarsBySymbol(symbol, startDate, endDate);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(mockBars, okResult.Value);
        }

        //[Fact]
        //public async Task GetHistoricalBarsBySymbol_ShouldReturnBadRequestOnException()
        //{
        //    // Arrange
        //    var symbol = "AAPL";
        //    var startDate = new DateTime(2023, 1, 1);
        //    var endDate = new DateTime(2023, 1, 2);
        //    var exceptionMessage = "An error occurred";

        //    _mockAlpacaRepository.Setup(repo => repo.GetHistoricalBars(symbol, startDate.ToUniversalTime(), endDate.ToUniversalTime()))
        //        .ThrowsAsync(new Exception(exceptionMessage));

        //    // Act
        //    var result = await _alpacaTestController.GetHistoricalBarsBySymbol(symbol, startDate, endDate);
        //    var badRequestResult = result as BadRequestObjectResult;

        //    // Assert
        //    Assert.NotNull(badRequestResult);
        //    Assert.Equal(400, badRequestResult.StatusCode);
        //    Assert.Equal(exceptionMessage, badRequestResult.Value);
        //}

        [Fact]
        public async Task RunBacktest_ShouldReturnOkWithResult()
        {
            // Arrange
            var settings = new StrategySettingsModel
            {
                Name = "Test Strategy",
                Broker = "Alpaca",
                Quantity = 10,
                StartDate = new DateTime(2023, 1, 1),
                EndDate = new DateTime(2023, 1, 2),
                Asset = "AAPL",
                StrategyType = StrategyEnum.Breakout,
                UserId = Guid.NewGuid(),
            };
            var result = "true";

            _mockStrategyServiceClient.Setup(client => client.StartStrategyAsync(settings))
                .ReturnsAsync(result);

            _mockBacktestService.Setup(service => service.RunBacktest(settings));

            // Act
            var actionResult = await _alpacaTestController.RunBacktest(settings);
            var okResult = actionResult as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(result, okResult.Value);
        }

        [Fact]
        public async Task RunBacktest_ShouldReturnBadRequestOnErrorCode()
        {
            // Arrange
            var settings = new StrategySettingsModel
            {
                Name = "Test Strategy",
                Broker = "Alpaca",
                Quantity = 10,
                StartDate = new DateTime(2023, 1, 1),
                EndDate = new DateTime(2023, 1, 2),
                Asset = "AAPL",
                StrategyType = StrategyEnum.Breakout,
                UserId = Guid.NewGuid(),
            };
            var result = "BadRequest";

            _mockStrategyServiceClient.Setup(client => client.StartStrategyAsync(settings))
                .ReturnsAsync(result);
            _mockBacktestService.Setup(service => service.RunBacktest(settings));
            // Act
            var actionResult = await _alpacaTestController.RunBacktest(settings);
            var badRequestResult = actionResult as BadRequestObjectResult;

            // Assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal(Enum.Parse(typeof(BnErrorCode), result), badRequestResult.Value);
        }
    }
}