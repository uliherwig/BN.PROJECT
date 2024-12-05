using BN.PROJECT.AlpacaService;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;

namespace BN.PROJECT.AlpacaServiceTests
{
    public class AlpacaTradingControllerTests
    {
        private readonly Mock<IAlpacaRepository> _mockRepo;
        private readonly Mock<IAlpacaTradingService> _mockTradingService;
        private readonly AlpacaTradingController _alpacaTradingController;

        public AlpacaTradingControllerTests()
        {
            _mockRepo = new Mock<IAlpacaRepository>();

            _mockRepo.Setup(repo => repo.GetAssets()).ReturnsAsync(new List<AlpacaAsset>
            {
                new AlpacaAsset { Symbol = "A", Name="A", AssetId = Guid.NewGuid() },
                new AlpacaAsset { Symbol = "B", Name="B", AssetId = Guid.NewGuid() },
                new AlpacaAsset { Symbol = "C", Name="C", AssetId = Guid.NewGuid() },
                new AlpacaAsset { Symbol = "D", Name="D", AssetId = Guid.NewGuid()  }
            });
            _mockTradingService = new Mock<IAlpacaTradingService>();

            _alpacaTradingController = new AlpacaTradingController(_mockTradingService.Object, _mockRepo.Object);

        }

        [Fact]
        public async Task GetAccount_ShouldReturnBadRequestUserIdCannotBeNull()
        {
            var result = await _alpacaTradingController.GetAccount(string.Empty);
            var badResult = result as BadRequestObjectResult;
            Assert.NotNull(badResult);
        }


        [Fact]
        public async Task GetAllAssetsAsync()
        {
            var result = await _alpacaTradingController.GetAssets();
            var okResult = result as OkObjectResult;
            var assets = okResult.Value as List<AlpacaAsset>;

            Assert.Equal(4, assets.Count);

        }



    }
}
