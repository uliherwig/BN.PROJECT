namespace BN.PROJECT.Core;

public class BreakoutModel
{
    public StopLossTypeEnum StopLossType { get; set; } = StopLossTypeEnum.Breakout;
    public BreakoutPeriodEnum BreakoutPeriod { get; set; } = BreakoutPeriodEnum.Day;
}