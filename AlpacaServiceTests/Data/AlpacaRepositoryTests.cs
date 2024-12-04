using BN.PROJECT.AlpacaService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace BN.PROJECT.AlpacaServiceTests
{
    public class AlpacaRepositoryTests
    {
        private readonly Mock<IAlpacaRepository> _mockRepo;
        private readonly Mock<IAlpacaTradingService> _mockTradingService;
        private readonly AlpacaTradingController _alpacaTradingController;

        public AlpacaRepositoryTests()
        {
            _mockRepo = new Mock<IAlpacaRepository>();

            _mockRepo.Setup(repo => repo.GetAssets()).ReturnsAsync(new List<AlpacaAsset>
            {
                new AlpacaAsset { Symbol = "C", Name="C", AssetId = Guid.NewGuid() },
                new AlpacaAsset { Symbol = "D", Name="D", AssetId = Guid.NewGuid()  }
            });
            _mockTradingService = new Mock<IAlpacaTradingService>();

            _alpacaTradingController = new AlpacaTradingController(_mockTradingService.Object, _mockRepo.Object);

        }

        [Fact]
        public async Task GetAllAssetsAsync()
        {
            var result = await _alpacaTradingController.GetAssets();
            var okResult = result as OkObjectResult;
            var assets = okResult.Value as List<AlpacaAsset>;

            Assert.Equal(2, assets.Count);

        }



    }
}
