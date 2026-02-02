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

}
