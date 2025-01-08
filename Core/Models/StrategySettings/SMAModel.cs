namespace BN.PROJECT.Core;

public class SmaModel
{
    public int ShortPeriod { get; set; } = 20;
    public int LongPeriod { get; set; } = 30;
    public StopLossTypeEnum StopLossType { get; set; }
    public decimal IntersectionThreshold { get; set; }
}