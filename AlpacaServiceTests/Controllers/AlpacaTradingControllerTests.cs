using BN.PROJECT.Core;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BN.PROJECT.AlpacaService.Tests
{
    public class AlpacaTradingControllerTests
    {
        private readonly Mock<IAlpacaRepository> _mockAlpacaRepository;
        private readonly Mock<IAlpacaTradingService> _mockTradingService;
        private readonly AlpacaTradingController _alpacaTradingController;
        private readonly Mock<IStrategyTestService> _mockBacktestService;
        private readonly Mock<IStrategyServiceClient> _mockStrategyServiceClient;

        public AlpacaTradingControllerTests()
        {
            _mockAlpacaRepository = new Mock<IAlpacaRepository>();
            _mockTradingService = new Mock<IAlpacaTradingService>();
            _mockBacktestService = new Mock<IStrategyTestService>();
            _mockStrategyServiceClient = new Mock<IStrategyServiceClient>();
            _alpacaTradingController = new AlpacaTradingController(
                _mockTradingService.Object,
                _mockAlpacaRepository.Object,
                _mockBacktestService.Object,
                _mockStrategyServiceClient.Object);
        }

        [Fact]
        public async Task GetAccount_ShouldReturnBadRequestUserIdCannotBeNull()
        {
            var result = await _alpacaTradingController.GetAccount();
            var badResult = result as BadRequestObjectResult;
            Assert.NotNull(badResult);
        }

        [Fact]
        public async Task GetAccount_ShouldReturnBadRequestUserSettingsNotFound()
        {
            var result = await _alpacaTradingController.GetAccount();
            var okResult = result as OkObjectResult;
            var account = okResult?.Value as BrokerAccount;

            Assert.Equal(AccountStatusEnum.NoCredentials, account?.AccountStatus);
        }

        [Fact]
        public async Task GetAccount_ShouldReturnBadRequestAlpacaAccountNotFound()
        {
            var userSettings = new UserSettingsModel
            {
                AlpacaSecret = "secret",
                AlpacaKey = "key"
            };

            _mockAlpacaRepository.Setup(repo => repo.GetUserSettingsAsync(It.IsAny<string>()))
                .ReturnsAsync(userSettings);

            var result = await _alpacaTradingController.GetAccount();
            var okResult = result as OkObjectResult;
            var account = okResult?.Value as BrokerAccount;

            Assert.Equal(AccountStatusEnum.WrongCredentials, account?.AccountStatus);
        }

        [Fact]
        public async Task GetAllAssetsAsync()
        {
            var assets = new List<AlpacaAsset>
            {
                new AlpacaAsset { Symbol = "AAPL" },
                new AlpacaAsset { Symbol = "GOOGL" },
                new AlpacaAsset { Symbol = "TSLA" },
                new AlpacaAsset { Symbol = "AMZN" }
            };
            _mockAlpacaRepository.Setup(repo => repo.GetAssets())
                .ReturnsAsync(assets);
            var result = await _alpacaTradingController.GetAssets();
            var okResult = result as OkObjectResult;
            var foundAssets = okResult.Value as List<AlpacaAsset>;

            Assert.Equal(4, foundAssets.Count);

        }
    }
}