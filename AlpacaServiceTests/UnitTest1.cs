using BN.PROJECT.AlpacaService;
using Microsoft.EntityFrameworkCore;

namespace AlpacaServiceTests
{
    public class UnitTest1
    {
        [Fact]
        public async void Test1()
        {
            // Example with InMemoryDatabase
            var optionsBuilder = new DbContextOptionsBuilder<AlpacaDbContext>()
                .UseInMemoryDatabase(databaseName: "AlpacaDb");
            var context = new AlpacaDbContext(optionsBuilder.Options);

            var asset = new AlpacaAsset
            {
                Symbol = "AAPL",
                Name = "Apple Inc",
                AssetId = Guid.NewGuid()
            };

            var repo = new AlpacaRepository(context);
            await repo.AddAssetsAsync(new List<AlpacaAsset> { asset });
            Assert.Equal(asset.Symbol, context.Assets.First().Symbol);



        }

        [Fact]
        public async void Test2()
        {
            var options = new DbContextOptionsBuilder<AlpacaDbContext>()
                 .UseInMemoryDatabase(databaseName: "AlpacaDb");


            using var context = new AlpacaDbContext(options.Options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            var asset = new AlpacaAsset
            {
                Symbol = "AAPL",
                Name = "Apple Inc",
                AssetId = Guid.NewGuid()
            };
            var repo = new AlpacaRepository(context);
            await repo.AddAssetsAsync(new List<AlpacaAsset> { asset });
            Assert.Equal(asset.Symbol, context.Assets.First().Symbol);


        }
    }
}