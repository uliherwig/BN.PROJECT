using BN.PROJECT.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BN.PROJECT.StrategyService.Tests;

public class StrategyControllerTests
{
    private readonly Mock<ILogger<StrategyController>> _mockLogger;
    private readonly Mock<IStrategyRepository> _mockStrategyRepository;
    private readonly StrategyController _controller;

    public StrategyControllerTests()
    {
        _mockLogger = new Mock<ILogger<StrategyController>>();
        _mockStrategyRepository = new Mock<IStrategyRepository>();
        _controller = new StrategyController(_mockLogger.Object, _mockStrategyRepository.Object);
    }

    [Fact]
    public async Task GetStrategyById_ReturnsOkResult_WithStrategy()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var strategy = new StrategySettingsModel { Id = strategyId };

        _mockStrategyRepository.Setup(repo => repo.GetStrategyByIdAsync(strategyId))
            .ReturnsAsync(strategy);

        // Act
        var result = await _controller.GetStrategyById(strategyId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(strategy, okResult.Value);
    }

    [Fact]
    public async Task GetStrategyNameExists_ReturnsOkResult_WithBoolean()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = "TestStrategy";
        var strategies = new List<StrategySettingsModel>
        {
            new StrategySettingsModel { Name = name }
        };
        _mockStrategyRepository.Setup(repo => repo.GetStrategiesByUserIdAsync(userId, false))
            .ReturnsAsync(strategies);

        // Act
        var result = await _controller.GetStrategyNameExists(name, userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value);
    }

    [Fact]
    public async Task AddStrategy_ReturnsOkResult()
    {
        // Arrange
        var strategy = new StrategySettingsModel();

        // Act
        var result = await _controller.AddStrategy(strategy);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value);
    }

    [Fact]
    public async Task DeleteTestAndPositions_ReturnsOkResult()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var strategy = new StrategySettingsModel { Id = strategyId };
        var positions = new List<PositionModel>();
        _mockStrategyRepository.Setup(repo => repo.GetStrategyByIdAsync(strategyId))
            .ReturnsAsync(strategy);
        _mockStrategyRepository.Setup(repo => repo.GetPositionsByStrategyIdAsync(strategyId))
            .ReturnsAsync(positions);

        // Act
        var result = await _controller.DeleteTestAndPositions(strategyId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value);
    }

    [Fact]
    public async Task UpdateStrategy_ReturnsOkResult()
    {
        // Arrange
        var strategy = new StrategySettingsModel();

        // Act
        var result = await _controller.UpdateStrategy(strategy);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value);
    }

    [Fact]
    public async Task GetStrategiesByUserId_ReturnsOkResult_WithStrategies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var strategies = new List<StrategySettingsModel>();
        _mockStrategyRepository.Setup(repo => repo.GetStrategiesByUserIdAsync(userId, false))
            .ReturnsAsync(strategies);

        // Act
        var result = await _controller.GetStrategiesByUserId(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(strategies, okResult.Value);
    }

    [Fact]
    public async Task GetStrategyInfosByUserId_ReturnsOkResult_WithStrategyInfos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var strategyType = StrategyEnum.None;
        var strategies = new List<StrategySettingsModel>();
        _mockStrategyRepository.Setup(repo => repo.GetStrategiesByUserIdAsync(userId, false))
            .ReturnsAsync(strategies);

        // Act
        var result = await _controller.GetStrategyInfosByUserId(userId, strategyType);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var strategyInfos = strategies.Select(s => new StrategyInfo
        {
            Id = s.Id,
            Label = s.Name,
        }).ToList();
        Assert.Equal(strategyInfos, okResult.Value);
    }

    [Fact]
    public async Task GetTestPositionsByStrategyId_ReturnsOkResult_WithPositions()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var positions = new List<PositionModel>
        {
            new PositionModel { PriceClose = 100 }
        };
        _mockStrategyRepository.Setup(repo => repo.GetPositionsByStrategyIdAsync(strategyId))
            .ReturnsAsync(positions);

        // Act
        var result = await _controller.GetTestPositionsByStrategyId(strategyId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(positions, okResult.Value);
    }

    [Fact]
    public async Task GetTestResultsByStrategyId_ReturnsOkResult_WithTestResult()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var strategy = new StrategySettingsModel
        {
            Id = strategyId,
            Asset = "Asset",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow
        };
        var positions = new List<PositionModel>
        {
            new PositionModel { PriceClose = 100, Side = SideEnum.Buy, ProfitLoss = 10 }
        };
        _mockStrategyRepository.Setup(repo => repo.GetStrategyByIdAsync(strategyId))
            .ReturnsAsync(strategy);
        _mockStrategyRepository.Setup(repo => repo.GetPositionsByStrategyIdAsync(strategyId))
            .ReturnsAsync(positions);

        // Act
        var result = await _controller.GetTestResultsByStrategyId(strategyId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var testResult = new TestResult
        {
            Id = strategyId,
            Asset = strategy.Asset,
            StartDate = strategy.StartDate,
            EndDate = strategy.EndDate,
            NumberOfPositions = positions.Count,
            NumberOfBuyPositions = positions.Count(p => p.Side == SideEnum.Buy),
            NumberOfSellPositions = positions.Count(p => p.Side == SideEnum.Sell),
            TotalProfitLoss = positions.Sum(p => p.ProfitLoss),
            BuyProfitLoss = positions.Where(p => p.Side == SideEnum.Buy).Sum(p => p.ProfitLoss),
            SellProfitLoss = positions.Where(p => p.Side == SideEnum.Sell).Sum(p => p.ProfitLoss)
        };
        var asset = okResult.Value.GetType().GetProperty("Asset").GetValue(okResult.Value);
        Assert.Equal(strategy.Asset, asset);
    }
}
