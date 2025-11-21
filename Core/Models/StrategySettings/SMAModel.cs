namespace BN.PROJECT.Core;

public class SmaModel
{
    public int ShortPeriod { get; set; } = 20;
    public int LongPeriod { get; set; } = 30;
    public int SlopeWindow { get; set; } = 5;
    public decimal MinSlopeThreshold { get; set; } = 0.01m;
    public StopLossTypeEnum StopLossType { get; set; }


}