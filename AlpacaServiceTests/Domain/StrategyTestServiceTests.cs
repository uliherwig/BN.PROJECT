using BN.PROJECT.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;

namespace BN.PROJECT.AlpacaService.Tests
{
    public class StrategyTestServiceTests
    {
        private readonly Mock<IAlpacaRepository> _mockAlpacaRepository;
        private readonly Mock<IAlpacaTradingService> _mockAlpacaTradingService;
        private readonly Mock<IKafkaProducerService> _mockKafkaProducer;
        private readonly Mock<IStrategyServiceClient> _mockStrategyServiceClient;
        private readonly Mock<ILogger<StrategyTestService>> _mockLogger;
        private readonly StrategyTestService _backtestService;
        private readonly Mock<IHubContext<AlpacaHub>> _mockHubContext; // Mock for IHubContext<AlpacaHub>
        private readonly Mock<IConnectionMultiplexer> _mockRedisDatabase; // Mock for IConnectionMultiplexer
        private readonly Mock<IRedisPublisher> _mockRedisPublisher;

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

        public StrategyTestServiceTests()
        {
            _mockAlpacaRepository = new Mock<IAlpacaRepository>();
            _mockRedisDatabase = new Mock<IConnectionMultiplexer>();
            _mockAlpacaTradingService = new Mock<IAlpacaTradingService>();
            _mockStrategyServiceClient = new Mock<IStrategyServiceClient>();
            _mockHubContext = new Mock<IHubContext<AlpacaHub>>(); 
            _mockRedisPublisher = new Mock<IRedisPublisher>();
            _mockRedisPublisher.Setup(publisher => publisher.Publish(It.IsAny<string>(), It.IsAny<string>()))
                                 .Returns(Task.CompletedTask);

            _mockLogger = new Mock<ILogger<StrategyTestService>>();
            _backtestService = new StrategyTestService(_mockAlpacaRepository.Object, 
                _mockLogger.Object, 
                _mockStrategyServiceClient.Object,
                _mockAlpacaTradingService.Object,
                _mockHubContext.Object,
                _mockRedisDatabase.Object, 
                _mockRedisPublisher.Object);
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
            _mockRedisPublisher.Verify(producer => producer.PublishAsync(It.Is<string>(msg => msg.Contains("\"MessageType\":\"startTest\"")), ""));
            _mockRedisPublisher.Verify(producer => producer.PublishAsync(It.Is<string>(msg => msg.Contains("\"MessageType\":\"stopTest\"")), ""));
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
       }
    }
}