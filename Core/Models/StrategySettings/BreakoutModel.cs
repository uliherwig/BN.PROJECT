namespace BN.PROJECT.Core;

public class BreakoutModel
{
    public StopLossTypeEnum StopLossType { get; set; } = StopLossTypeEnum.Breakout;
    public TimeFrameEnum BreakoutPeriod { get; set; } = TimeFrameEnum.Day;
}