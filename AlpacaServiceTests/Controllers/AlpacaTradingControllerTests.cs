using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BN.PROJECT.AlpacaService.Tests
{
    public class AlpacaTradingControllerTests
    {
        private readonly Mock<IAlpacaRepository> _mockAlpacaRepository;
        private readonly Mock<IAlpacaTradingService> _mockTradingService;
        private readonly AlpacaTradingController _alpacaTradingController;

        public AlpacaTradingControllerTests()
        {
            _mockAlpacaRepository = new Mock<IAlpacaRepository>();
            _mockTradingService = new Mock<IAlpacaTradingService>();
            _alpacaTradingController = new AlpacaTradingController(_mockTradingService.Object, _mockAlpacaRepository.Object);
        }

        [Fact]
        public async Task GetAccount_ShouldReturnBadRequestUserIdCannotBeNull()
        {
            var result = await _alpacaTradingController.GetAccount(string.Empty);
            var badResult = result as BadRequestObjectResult;
            Assert.NotNull(badResult);
        }

        [Fact]
        public async Task GetAccount_ShouldReturnBadRequestUserSettingsNotFound()
        {
            var result = await _alpacaTradingController.GetAccount("456");
            var badResult = result as BadRequestObjectResult;
            Assert.NotNull(badResult);
        }

        //[Fact]
        //public async Task GetAccount_ShouldReturnBadRequestAlpacaAccountNotFound()
        //{
        //    var result = await _alpacaTradingController.GetAccount("123");
        //    var notFoundObject = result as NotFoundObjectResult;
        //    Assert.NotNull(notFoundObject);
        //}

        //[Fact]
        //public async Task GetAllAssetsAsync()
        //{
        //    var result = await _alpacaTradingController.GetAssets();
        //    var okResult = result as OkObjectResult;
        //    var assets = okResult.Value as List<AlpacaAsset>;

        //    Assert.Equal(4, assets.Count);

        //}
    }
}