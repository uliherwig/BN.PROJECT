using BN.PROJECT.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace BN.PROJECT.AlpacaService.Tests
{
    public class BacktestServiceTests
    {
        private readonly Mock<IAlpacaRepository> _mockAlpacaRepository;
        private readonly Mock<IKafkaProducerHostedService> _mockKafkaProducer;
        private readonly Mock<ILogger<BacktestService>> _mockLogger;
        private readonly BacktestService _backtestService;

        private readonly StrategySettingsModel _settings = new()
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

        public BacktestServiceTests()
        {
            _mockAlpacaRepository = new Mock<IAlpacaRepository>();
            _mockKafkaProducer = new Mock<IKafkaProducerHostedService>();

            _mockKafkaProducer.Setup(producer => producer.SendMessageAsync("strategy", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

            _mockLogger = new Mock<ILogger<BacktestService>>();
            _backtestService = new BacktestService(_mockAlpacaRepository.Object, _mockLogger.Object, _mockKafkaProducer.Object);
        }

        [Fact]
        public async Task RunBacktest_ShouldSendStartTestMessage()
        {
            // Arrange
            _mockAlpacaRepository.Setup(repo => repo.GetHistoricalBars(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<AlpacaBar>());

            // Act
            await _backtestService.RunBacktest(_settings);

            // Assert
            _mockKafkaProducer.Verify(producer => producer.SendMessageAsync("strategy", It.Is<string>(msg => msg.Contains("\"MessageType\":\"startTest\"")), It.IsAny<CancellationToken>()));
            _mockKafkaProducer.Verify(producer => producer.SendMessageAsync("strategy", It.Is<string>(msg => msg.Contains("\"MessageType\":\"stopTest\"")), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task RunBacktest_ShouldSendQuotesMessages()
        {
            // Arrange
            var symbol = "AAPL";
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 2);
            var mockBars = new List<AlpacaBar>
            {
                new() { T = startDate, O = 150, H = 155, L = 149, C = 152, V = 1000, Symbol = symbol },
                new() { T = endDate, O = 152, H = 156, L = 151, C = 154, V = 1200, Symbol = symbol }
            };

            _mockAlpacaRepository.Setup(repo => repo.GetHistoricalBars(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(mockBars);

            // Act
            await _backtestService.RunBacktest(_settings);

            // Assert
            _mockKafkaProducer.Verify(producer => producer.SendMessageAsync("strategy", It.Is<string>(msg => msg.Contains("\"MessageType\":\"startTest\"")), It.IsAny<CancellationToken>()));
            _mockKafkaProducer.Verify(producer => producer.SendMessageAsync("strategy", It.Is<string>(msg => msg.Contains("\"MessageType\":\"quotes\"")), It.IsAny<CancellationToken>()));
            _mockKafkaProducer.Verify(producer => producer.SendMessageAsync("strategy", It.Is<string>(msg => msg.Contains("\"MessageType\":\"stopTest\"")), It.IsAny<CancellationToken>()));
        }
    }
}