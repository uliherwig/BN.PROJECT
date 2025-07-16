namespace BN.PROJECT.Core;

public class StrategyMessage
{
    public Guid UserId { get; set; }
    public Guid StrategyId { get; set; }
    public StrategyTaskEnum StrategyTask { get; set; } = StrategyTaskEnum.None;
    public bool IsBacktest { get; set; } = false;
    public StrategyEnum Strategy { get; set; } = StrategyEnum.None;
    public MessageTypeEnum MessageType { get; set; } = MessageTypeEnum.Start;
    public StrategySettingsModel? Settings { get; set; } = null;
    public List<Quote>? Quotes { get; set; } = null;
}