namespace BN.PROJECT.AlpacaService.Tests
{
    public class AlpacaRepositoryTests
    {
        private readonly AlpacaDbContext _dbContext;
        private readonly DateTime _testStamp = new DateTime(2024, 10, 11, 17, 12, 1);

        public AlpacaRepositoryTests()
        {
            _dbContext = DatabaseGenerator.SeedDataBase();
        }

        [Fact]
        public async Task GetAssets_ReturnsOrderedAssets()
        {
            // Arrange
            var repository = new AlpacaRepository(_dbContext);

            // Act
            var result = await repository.GetAssets();

            // Assert
            Assert.Equal("A", result[0].Symbol);
            Assert.Equal("B", result[1].Symbol);
            Assert.Equal("C", result[2].Symbol);
        }

        [Fact]
        public async Task AddAssetsAsync_AddsAssetsToDatabase()
        {
            // Arrange
            var repository = new AlpacaRepository(_dbContext);
            var assets = new List<AlpacaAsset>
            {
                new() { Symbol = "X", Name = "Apple Inc", AssetId = Guid.NewGuid() },
                new() { Symbol = "Y", Name = "Microsoft Corporation", AssetId = Guid.NewGuid() }
            };

            // Act
            await repository.AddAssetsAsync(assets);

            // Assert
            Assert.Equal(6, _dbContext.Assets.Count());
            Assert.Equal("X", _dbContext.Assets.First(x => x.Symbol == "X").Symbol);
        }

        [Fact]
        public async Task GetAsset_ReturnsAsset()
        {
            // Arrange
            var repository = new AlpacaRepository(_dbContext);

            // Act
            var result = await repository.GetAsset("A");

            // Assert
            Assert.Equal("A", result.Symbol);
        }

        [Fact]
        public async Task GetLatestBar_ReturnsLatestBar()
        {
            // Arrange
            var repository = new AlpacaRepository(_dbContext);

            // Act
            var result = await repository.GetLatestBar("A");

            // Assert
            Assert.Equal("A", result.Symbol);
        }

        [Fact]
        public async Task GetHistoricalBars_ReturnsHistoricalBars()
        {
            // Arrange
            var repository = new AlpacaRepository(_dbContext);

            // Act
            var result = await repository.GetHistoricalBars("A", _testStamp.AddHours(-1), _testStamp.AddHours(1));

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal("A", result[0].Symbol);
        }

        [Fact]
        public async Task AddBarsAsync_AddsBarsToDatabase()
        {
            // Arrange
            var repository = new AlpacaRepository(_dbContext);
            var bars = new List<AlpacaBar>
            {
                new AlpacaBar { Symbol = "AAPL", T = DateTime.Now, O = 1, H = 2, L = 3, C = 4, V = 5 },
                new AlpacaBar { Symbol = "MSFT", T = DateTime.Now, O = 1, H = 2, L = 3, C = 4, V = 5 }
            };

            // Act
            await repository.AddBarsAsync(bars);

            // Assert
            var count = _dbContext.Bars.Count();
            Assert.Equal(5, count);
            Assert.Equal("MSFT", _dbContext.Bars.First(b => b.Symbol == "MSFT").Symbol);
        }

        [Fact]
        public async Task GetLatestQuote_ReturnsLatestQuote()
        {
            // Arrange
            var repository = new AlpacaRepository(_dbContext);

            // Act
            var result = await repository.GetLatestQuote("A");

            // Assert
            Assert.Equal("A", result.Symbol);
        }

        [Fact]
        public async Task GetHistoricalQuotes_ReturnsHistoricalQuotes()
        {
            // Arrange
            var repository = new AlpacaRepository(_dbContext);

            // Act
            var result = await repository.GetHistoricalQuotes("A", _testStamp.AddHours(-1), _testStamp.AddHours(1));

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal("A", result.First().Symbol);
        }

        [Fact]
        public async Task AddUserSettingsAsync_AddsUserSettingsToDatabase()
        {
            // Arrange
            var repository = new AlpacaRepository(_dbContext);
            var userSettings = new UserSettingsModel { UserId = "123", AlpacaKey = "123", AlpacaSecret = "123" };

            // Act
            await repository.AddUserSettingsAsync(userSettings);

            // Assert
            Assert.Equal(1, _dbContext.UserSettings.Count());
            Assert.Equal("123", _dbContext.UserSettings.First().UserId);
        }

        [Fact]
        public async Task GetUserSettingsAsync_ReturnsCorrectUserSettings()
        {
            // Arrange
            var repository = new AlpacaRepository(_dbContext);
            var userSettings = new UserSettingsModel { UserId = "123", AlpacaKey = "123", AlpacaSecret = "123" };
            _dbContext.UserSettings.Add(userSettings);
            _dbContext.SaveChanges();

            // Act
            var result = await repository.GetUserSettingsAsync("123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("123", result.UserId);
            Assert.Equal("123", result.AlpacaKey);
        }

        [Fact]
        public async Task UpdateUserSettingsAsync_UpdatesUserSettingsInDatabase()
        {
            // Arrange
            var repository = new AlpacaRepository(_dbContext);
            var userSettings = new UserSettingsModel { UserId = "123", AlpacaKey = "123", AlpacaSecret = "123" };
            _dbContext.UserSettings.Add(userSettings);
            _dbContext.SaveChanges();

            // Act
            userSettings.AlpacaKey = "456";
            await repository.UpdateUserSettingsAsync(userSettings);

            // Assert
            var updatedSettings = _dbContext.UserSettings.First(u => u.UserId == "123");
            Assert.Equal("456", updatedSettings.AlpacaKey);
        }

        [Fact]
        public async Task DeleteUserSettingsAsync_DeletesUserSettingsFromDatabase()
        {
            // Arrange
            var repository = new AlpacaRepository(_dbContext);
            var userSettings = new UserSettingsModel { UserId = "123", AlpacaKey = "123", AlpacaSecret = "123" };
            _dbContext.UserSettings.Add(userSettings);
            _dbContext.SaveChanges();

            // Act
            await repository.DeleteUserSettingsAsync(userSettings);

            // Assert
            Assert.Equal(0, _dbContext.UserSettings.Count());
        }
    }
}