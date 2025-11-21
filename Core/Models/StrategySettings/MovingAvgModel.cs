namespace BN.PROJECT.Core;

public class MovingAvgModel
{
    public StrategyEnum Strategy { get; set; } = StrategyEnum.SMA;
    public int ShortPeriod { get; set; } = 30;
    public int LongPeriod { get; set; } = 60;
    public int SlopeWindow { get; set; } = 5;
    public decimal MinSlopeThreshold { get; set; } = 0.01m;
    public StopLossTypeEnum StopLossType { get; set; }
    public decimal IntersectionThreshold { get; set; }
}