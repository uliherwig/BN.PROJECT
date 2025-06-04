using Microsoft.Extensions.Logging;
using Moq;
using BN.PROJECT.Core;

namespace BN.PROJECT.StrategyService.Tests;

public class StrategyRepositoryTests
{
    private readonly Mock<ILogger<StrategyRepository>> _mockLogger;
    private readonly StrategyDbContext _dbContext;

    public StrategyRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<StrategyRepository>>();
        _dbContext = DataBaseGenerator.SeedDataBase();
    }

    [Fact]
    public async Task GetStrategiesByUserIdAsync_ReturnsStrategies()
    {
        // Arrange
        var repository = new StrategyRepository(_dbContext, _mockLogger.Object);

        // Act
        var result = await repository.GetStrategiesByUserIdAsync(DataBaseGenerator.TestGuid, false);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Strategy 1", result[1].Name);
        Assert.Equal("Strategy 2", result[0].Name);
    }

    [Fact]
    public async Task GetStrategyByIdAsync_ReturnsStrategy()
    {
        // Arrange
        var repository = new StrategyRepository(_dbContext, _mockLogger.Object);
       

        // Act
        var result = await repository.GetStrategyByIdAsync(DataBaseGenerator.TestGuid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DataBaseGenerator.TestGuid, result.Id);
    }

    [Fact]
    public async Task AddStrategyAsync_AddsStrategyToDatabase()
    {
        // Arrange
        var repository = new StrategyRepository(_dbContext, _mockLogger.Object);
        var strategy = new StrategySettingsModel
        {
            Name = "Strategy 3",
            Broker = "Alpaca",
            Quantity = 10,
            StartDate = new DateTime(2023, 1, 1),
            EndDate = new DateTime(2023, 1, 2),
            Asset = "AAPL",
            StrategyType = StrategyEnum.Breakout,
            UserId = Guid.NewGuid(),
        };

        // Act
        await repository.AddStrategyAsync(strategy);

        // Assert
        Assert.Equal(3, _dbContext.Strategies.Count());
        Assert.Equal("Strategy 3", _dbContext.Strategies.First(x => x.Name == "Strategy 3").Name);
    }

    [Fact]
    public async Task UpdateStrategyAsync_UpdatesStrategyInDatabase()
    {
        // Arrange
        var repository = new StrategyRepository(_dbContext, _mockLogger.Object);
        var strategy = _dbContext.Strategies.First(x => x.Id == DataBaseGenerator.TestGuid);
        strategy.Name = "Strategy 123";

        // Act
        await repository.UpdateStrategyAsync(strategy);

        // Assert
        Assert.Equal(2, _dbContext.Strategies.Count());
        Assert.Equal("Strategy 123", _dbContext.Strategies.First(x => x.Id == DataBaseGenerator.TestGuid).Name);
    }

    [Fact]
    public async Task GetPositionsByStrategyIdAsync_ReturnsPositions()
    {
        // Arrange
        var repository = new StrategyRepository(_dbContext, _mockLogger.Object);    

        // Act
        var result = await repository.GetPositionsByStrategyIdAsync(DataBaseGenerator.TestGuid);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal("AAPL", result[0].Symbol);
    }

    [Fact]
    public async Task AddPositionAsync_AddsPositionToDatabase()
    {
        // Arrange
        var repository = new StrategyRepository(_dbContext, _mockLogger.Object);
        var position = new PositionModel
        {
            Id = Guid.NewGuid(),
            StrategyId = Guid.NewGuid(),
            Symbol = "SPY",
            Quantity = 1,
            Side = SideEnum.Buy,
            PriceOpen = 100,
            PriceClose = 110,
            ProfitLoss = 10,
            StampClosed = new DateTime(2023, 1, 2),
            TakeProfit = 110,
            StopLoss = 90,
            StampOpened = new DateTime(2023, 1, 2),
            CloseSignal = "",
        };

        // Act
        await repository.AddPositionAsync(position);

        // Assert
        Assert.Equal(3, _dbContext.Positions.Count());
        Assert.Equal("SPY", _dbContext.Positions.First(x => x.Symbol == "SPY").Symbol);
    }

    [Fact]
    public async Task UpdatePositionAsync_UpdatesPositionInDatabase()
    {
        // Arrange
        var repository = new StrategyRepository(_dbContext, _mockLogger.Object);
        var position = _dbContext.Positions.First(x => x.Id == DataBaseGenerator.TestGuid);
        position.CloseSignal = "Test";
        // Act
        await repository.UpdatePositionAsync(position);

        // Assert
        Assert.Equal(2, _dbContext.Positions.Count());
        Assert.Equal("Test", _dbContext.Positions.First(x => x.Id == DataBaseGenerator.TestGuid).CloseSignal);
    }

    [Fact]
    public async Task DeletePositionsAsync_DeletesPositionsFromDatabase()
    {
        // Arrange
        var repository = new StrategyRepository(_dbContext, _mockLogger.Object);

        var positions = _dbContext.Positions.ToList();

        // Act
        await repository.DeletePositionsAsync(positions);

        // Assert
        Assert.Equal(0, _dbContext.Positions.Count());
    }

    [Fact]
    public async Task DeleteStrategyAsync_DeletesStrategyFromDatabase()
    {
        // Arrange
        var repository = new StrategyRepository(_dbContext, _mockLogger.Object);
        var strategies = _dbContext.Strategies.ToList();

        // Act
        await repository.DeleteStrategyAsync(strategies.First());

        // Assert
        Assert.Equal(1, _dbContext.Strategies.Count());
    }
}