using Alpaca.Markets;
using BN.PROJECT.AlpacaService;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Dynamic;

namespace BN.PROJECT.AlpacaServiceTests
{
    public class AlpacaTradingControllerTests
    {
        private readonly IAlpacaRepository _alpacaRepository;
        private readonly Mock<IAlpacaTradingService> _mockTradingService;
        private readonly AlpacaTradingController _alpacaTradingController;

        public AlpacaTradingControllerTests()
        {
            var dbContext = DatabaseGenerator.CreateContext();
            DatabaseGenerator.SeedDataBase();
            _alpacaRepository = new AlpacaRepository(dbContext);       
            _mockTradingService = new Mock<IAlpacaTradingService>();
            var accountId = new  Guid("1F621067-A2B3-4197-8112-473DACBEAE25");

            var account = new
            {
                AccountId = accountId,
                Status = AccountStatus.Active,
                Currency = "USD",
                Cash = 1000
            };
            var a = JsonConvert.DeserializeObject<IAccount>(account.ToJson());

            _mockTradingService.Setup<Task<IAccount>>(x => x.GetAccountAsync(It.IsAny<UserSettings>()))
                .ReturnsAsync(a);

            _alpacaTradingController = new AlpacaTradingController(_mockTradingService.Object, _alpacaRepository);

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
            var notFoundObject = result as NotFoundObjectResult;
            Assert.NotNull(notFoundObject);
        }
        [Fact]
        public async Task GetAccount_ShouldReturnBadRequestAlpacaAccountNotFound()
        {
            var result = await _alpacaTradingController.GetAccount("123");
            var notFoundObject = result as NotFoundObjectResult;
            Assert.NotNull(notFoundObject);
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
