namespace BN.PROJECT.Core;

public class StrategyMessage
{
    public Guid UserId { get; set; }
    public Guid StrategyId { get; set; }
    public bool IsBacktest { get; set; }
    public StrategyEnum Strategy { get; set; }
    public MessageTypeEnum MessageType { get; set; }
    public StrategySettingsModel? Settings { get; set; }
    public List<Quote>? Quotes { get; set; }
}