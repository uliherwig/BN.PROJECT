using BN.PROJECT.Core;

namespace BN.PROJECT.StrategyService.Tests;

public class StrategyOperationsTests
{


    [Fact]
    public void GetStartOfTimeSpan_ShouldReturnCorrectStartOfTimeSpan()
    {
        // Arrange
        var dateTime = new DateTime(2023, 10, 10, 15, 45, 0);
        var timeSpan = TimeSpan.FromHours(1);

        // Act
        var result = StrategyOperations.GetStartOfTimeSpan(dateTime, timeSpan);

        // Assert
        Assert.Equal(new DateTime(2023, 10, 10, 15, 0, 0), result);
    }

    [Theory]  
    [InlineData(TimeFrameEnum.Minute, 1)]
    [InlineData(TimeFrameEnum.TenMinutes, 10)]
    public void GetTimeSpanByBreakoutPeriod_ShouldReturnCorrectTimeSpan(TimeFrameEnum breakoutPeriod, int expectedMinutes)
    {
        // Act
        var result = StrategyOperations.GetTimeSpanByBreakoutPeriod(breakoutPeriod);

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(expectedMinutes), result);
    }

    [Fact]
    public void UpdateOrCloseOpenPosition_ShouldUpdateTakeProfitAndStopLossForBuy()
    {
        // Arrange
        var openPosition = new PositionModel { Side = SideEnum.Buy, TakeProfit = 100, StopLoss = 90 };
        var quote = new Quote { BidPrice = 110, AskPrice = 105, TimestampUtc = DateTime.UtcNow };
        decimal trailingStop = 5;
        decimal takeProfitPercent = 10;

        // Act
        TradingOperations.UpdateOrCloseOpenPosition(ref openPosition, quote, trailingStop, takeProfitPercent);

        // Assert
        Assert.Equal(121, openPosition.TakeProfit);
        Assert.Equal(104.5m, openPosition.StopLoss);
    }
}
