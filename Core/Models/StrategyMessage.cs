namespace BN.PROJECT.Core;

public class StrategyMessage
{
    public Guid TestId { get; set; }
    public MessageType Type { get; set; }
    public BacktestSettings? TestSettings { get; set; }
    public List<Quote>? Quotes { get; set; }
}