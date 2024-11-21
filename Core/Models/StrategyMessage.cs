namespace BN.PROJECT.Core;

public class StrategyMessage
{
    public Guid StrategyId { get; set; }
    public StrategyEnum Strategy { get; set; }
    public MessageTypeEnum MessageType { get; set; }
    public StrategySettingsModel? Settings { get; set; }
    public List<Quote>? Quotes { get; set; }
}