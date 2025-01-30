using Alpaca.Markets;
using Microsoft.Extensions.Logging;
using Moq;

namespace BN.PROJECT.AlpacaService.Tests
{
    public class AlpacaTradingServiceTests
    {
        private readonly Mock<IAlpacaClient> _mockAlpacaClient;
        private readonly Mock<IAlpacaTradingClient> _mockAlpacaTradingClient;

        private readonly Mock<ILogger<AlpacaTradingService>> _mockLogger;
        private readonly AlpacaTradingService _alpacaTradingService;

        public AlpacaTradingServiceTests()
        {
            _mockAlpacaClient = new Mock<IAlpacaClient>();
            _mockAlpacaTradingClient = new Mock<IAlpacaTradingClient>();

            // Setup the mock to return the mocked IAlpacaTradingClient
            _mockAlpacaClient.Setup(client => client.GetPrivateTradingClient(It.IsAny<UserSettings>()))
                .Returns(_mockAlpacaTradingClient.Object);

            _mockAlpacaClient.Setup(client => client.GetCommonTradingClient())
                 .Returns(_mockAlpacaTradingClient.Object);

            _mockLogger = new Mock<ILogger<AlpacaTradingService>>();
            _alpacaTradingService = new AlpacaTradingService(_mockAlpacaClient.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAccountAsync_ShouldReturnAccount()
        {
            // Arrange
            var userSettings = new UserSettings { UserId = "testUser", AlpacaKey = "key", AlpacaSecret = "secret" };
            var mockAccount = new Mock<IAccount>();

            _mockAlpacaTradingClient.Setup(client => client.GetAccountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockAccount.Object);

            // Act
            var result = await _alpacaTradingService.GetAccountAsync(userSettings);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockAccount.Object, result);
        }

        [Fact]
        public async Task GetAssetsAsync_ShouldReturnAssets()
        {
            // Arrange
            var mockAssets = new List<IAsset>
            {
                new Mock<IAsset>().Object,
                new Mock<IAsset>().Object
            };

            _mockAlpacaTradingClient.Setup(client => client.ListAssetsAsync(It.IsAny<AssetsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockAssets);

            // Act
            var result = await _alpacaTradingService.GetAssetsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockAssets.Count, result.Count);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnOrder()
        {
            // Arrange
            var userSettings = new UserSettings
            {
                UserId = "testUser",
                AlpacaKey = "123",
                AlpacaSecret = "456"
            };
            var orderId = "testOrder";
            var mockOrder = new Mock<IOrder>();

            _mockAlpacaTradingClient.Setup(client => client.GetOrderAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockOrder.Object);

            // Act
            var result = await _alpacaTradingService.GetOrderByIdAsync(userSettings, orderId);

            // Assert
            Assert.NotNull(result);
            var test = mockOrder.Object.ToAlpacaOrder();
            Assert.Equal(test.AssetId, result.AssetId);
        }

        [Fact]
        public async Task CancelOrderByIdAsync_ShouldReturnTrue()
        {
            // Arrange
            var userSettings = new UserSettings
            {
                UserId = "testUser",
                AlpacaKey = "123",
                AlpacaSecret = "456"
            };
            var orderId = Guid.NewGuid();

            _mockAlpacaTradingClient.Setup(client => client.CancelOrderAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _alpacaTradingService.CancelOrderByIdAsync(userSettings, orderId);

            // Assert
            Assert.True(result);
        }
    }
}